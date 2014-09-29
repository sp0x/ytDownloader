Imports System.Collections.Generic
Imports System.IO


Namespace Extraction
    Public Class MP3Frame
        Public Property ChannelMode As Integer
        Public Property BitRate As Integer
        Public Property FirstFrameHeader As UInteger
        Public Property MpegVersion As Integer
        Public Property SampleRate As Integer
    End Class

    Friend Class Mp3AudioExtractor
        Implements IAudioExtractor
#Region "Static variables"
        Shared mpeg1BitRate As Integer() = New Int32() {0, 32, 40, 48, 56, 64, _
             80, 96, 112, 128, 160, 192, _
             224, 256, 320, 0}
        Shared mpeg2XBitRate As Integer() = New Int32() {0, 8, 16, 24, 32, 40, _
                48, 56, 64, 80, 96, 112, _
                128, 144, 160, 0}
        Shared mpeg1SampleRate As Integer() = New Int32() {44100, 48000, 32000, 0}
        Shared mpeg20SampleRate As Integer() = New Int32() {22050, 24000, 16000, 0}
        Shared mpeg25SampleRate As Integer() = New Int32() {11025, 12000, 8000, 0}
#End Region

#Region "Variables"

        Private chunkBuffer As MemoryStream ' List(Of Byte())
        Private p_videoStream As FileStream
        Private ReadOnly frameOffsets As List(Of UInteger)
        Private ls_warnings As List(Of String)

        Private hasVbrHeader As Boolean
        Private isVbr As Boolean
        Private FrameInfo As New MP3Frame
        Private totalFrameLength As UInteger
        Private doWriteVbrHeader As Boolean
        Private delayWrite As Boolean
#End Region

#Region "Construction"
        Public Sub New(path As String)
            p_videoStream = New FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024)
            Me.ls_warnings = New List(Of String)()
            Me.chunkBuffer = New MemoryStream
            Me.frameOffsets = New List(Of UInteger)()
            Me.delayWrite = True
        End Sub
#End Region

#Region "Props"
        Public Property VideoStream As FileStream Implements IAudioExtractor.VideoStream
            Get
                Return p_videoStream
            End Get
            Set(value As FileStream)
                p_videoStream = value
            End Set
        End Property
        Public ReadOnly Property Warnings() As IEnumerable(Of String)
            Get
                Return Me.ls_warnings
            End Get
        End Property
#End Region

#Region "Freame writer"
        ''' <summary>
        ''' Writes the FLV Buffer, by extracting it to a MP3 Frame.
        ''' </summary>
        ''' <param name="chunk">The buffer to write</param>
        ''' <param name="timeStamp">Not used</param>
        ''' <remarks></remarks>
        Public Sub WriteChunk(chunk As Byte(), timeStamp As UInteger) Implements IAudioExtractor.WriteChunk
            Me.chunkBuffer.Write(chunk, 0, chunk.Length)
            Me.ParseMp3Frame(chunk)
            If Me.delayWrite AndAlso Me.totalFrameLength >= 65536 Then
                Me.delayWrite = False
            End If
            If Not Me.delayWrite Then
                Me.Flush()
            End If
        End Sub

        Private Sub Flush()
            Dim tmpBuff As Byte() = chunkBuffer.ToArray()

            Me.p_videoStream.Write(tmpBuff, 0, tmpBuff.Length)
            chunkBuffer.Dispose()
            chunkBuffer = New MemoryStream() 'Me.chunkBuffer.Clear()
        End Sub
#End Region

#Region "Disposition"

        Public Sub Dispose() Implements IDisposable.Dispose
            Me.Flush()

            If Me.doWriteVbrHeader Then
                p_videoStream.Seek(0, SeekOrigin.Begin)
                WriteVbrHeader(False)
            End If

            Me.p_videoStream.Dispose()
        End Sub
#End Region

#Region "Helpers"
        Private Shared Function GetFrameDataOffset(mpegVersion As Integer, channelMode As Integer) As Integer
            Return 4 + (If(mpegVersion = 3, (If(channelMode = 3, 17, 32)), (If(channelMode = 3, 9, 17))))
        End Function

        Private Shared Function GetFrameLength(mpegVersion As Integer, bitRate As Integer, sampleRate As Integer, padding As Integer) As Integer
            Dim vFlag As Int32 = If(mpegVersion = 3, 144, 72)
            bitRate = vFlag * bitRate
            Return Integer.Parse(Math.Truncate(bitRate / sampleRate) + padding)
        End Function
        ''' <summary>
        ''' Parses the main information from the frame
        ''' </summary>
        ''' <param name="header">The header flag</param>
        ''' <param name="mpegVersion"></param>
        ''' <param name="layer"></param>
        ''' <param name="bitrate"></param>
        ''' <param name="padding"></param>
        ''' <param name="channelMode"></param>
        ''' <param name="sampleRate"></param>
        ''' <remarks></remarks>
        Private Sub getMp3FrameInfo(ByRef header As ULong, ByRef mpegVersion As Int32, ByRef layer As Int32, ByRef bitrate As Int32, ByRef padding As Int32, _
                                    ByRef channelMode As Int32, ByRef sampleRate As Int32)
            mpegVersion = BitHelper.Read(header, 2)
            layer = BitHelper.Read(header, 2)
            BitHelper.Read(header, 1)
            bitrate = BitHelper.Read(header, 4)
            sampleRate = BitHelper.Read(header, 2)
            padding = BitHelper.Read(header, 1)
            BitHelper.Read(header, 1)
            channelMode = BitHelper.Read(header, 2)
        End Sub

        ''' <summary>
        ''' Gets ssamplerate, based on the version of MPEG
        ''' </summary>
        ''' <param name="mpgVersion"></param>
        ''' <param name="sampleRate"></param>
        ''' <remarks></remarks>
        Private Shared Sub calcSampleRate(mpgVersion As Int32, ByRef sampleRate As Int32)
            Select Case mpgVersion
                Case 2
                    sampleRate = mpeg20SampleRate(sampleRate)
                Case 3
                    sampleRate = mpeg1SampleRate(sampleRate)
                Case Else
                    sampleRate = mpeg25SampleRate(sampleRate)
            End Select
        End Sub
        ''' <summary>
        ''' Checks if the given frame is a VBR Header frame.
        ''' </summary>
        ''' <param name="buffer">The buffer for the frame</param>
        ''' <param name="frameOffsets">The list of frame offsets</param>
        ''' <param name="offset">The current offset</param>
        ''' <param name="mpgVersion">MPEG Version</param>
        ''' <param name="chanMode">Channel count</param>
        ''' <param name="mp3Extractor">The extractor to modify</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function checkForVBRHeader(buffer As Byte(), frameOffsets As List(Of UInteger), offset As Int32, mpgVersion As Int32, chanMode As Int32 _
                                                  , ByRef mp3Extractor As Mp3AudioExtractor)
            Dim isVbrHeaderFrame As Boolean = False
            If frameOffsets.Count = 0 Then 'No frames have been found 
                ' Check for an existing VBR header just to be safe (I haven't seen any in FLVs)
                Dim hdrOffset As Integer = offset + GetFrameDataOffset(mpgVersion, chanMode)

                If BigEndianBitConverter.ToUInt32(buffer, hdrOffset) = &H58696E67 Then
                    ' "Xing"
                    isVbrHeaderFrame = True
                    mp3Extractor.delayWrite = False
                    mp3Extractor.hasVbrHeader = True
                End If
            End If
            Return isVbrHeaderFrame
        End Function
#End Region

#Region "Parsing"
        Private Sub ParseMp3Frame(buffer As Byte())
            Dim offset As Integer = 0
            Dim length As Integer = buffer.Length

            While length >= 4
                Dim mpegVersion, sampleRate, channelMode As Integer
                Dim layer, bitRate, padding As Integer
                Dim header As ULong = CULng(BigEndianBitConverter.ToUInt32(buffer, offset)) << 32

                If BitHelper.Read(header, 11) <> &H7FF Then
                    Exit While
                End If

                getMp3FrameInfo(header, mpegVersion, layer, bitRate, padding, channelMode, sampleRate)

                If mpegVersion = 1 OrElse layer <> 1 OrElse bitRate = 0 OrElse bitRate = 15 OrElse sampleRate = 3 Then
                    Exit While
                End If

                bitRate = (If(mpegVersion = 3, mpeg1BitRate(bitRate), mpeg2XBitRate(bitRate))) * 1000
                calcSampleRate(mpegVersion, sampleRate)
                Dim frameLenght As Integer = GetFrameLength(mpegVersion, bitRate, sampleRate, padding)
                If frameLenght > length Then
                    Exit While
                End If

                ParseHeaderInformation(Me, offset, buffer, bitRate, mpegVersion, sampleRate, channelMode)


                Me.frameOffsets.Add(Me.totalFrameLength + offset)
                offset += frameLenght
                length -= frameLenght
            End While

            Me.totalFrameLength += buffer.Length
        End Sub

        Private Shared Sub ParseHeaderInformation(ByRef mp3Extractor As Mp3AudioExtractor, _
                                                  offset As Int32, buffer As Byte(), bitrate As Int32, mpegVer As Int32, sampleRate As Int32, channelMode As Int32)
            Dim isVbrHeaderFrame As Boolean = checkForVBRHeader(buffer, mp3Extractor.frameOffsets, offset, mpegVer, channelMode, mp3Extractor)
            If Not isVbrHeaderFrame Then
                With mp3Extractor.FrameInfo
                    If .BitRate = 0 Then
                        .BitRate = bitrate
                        .MpegVersion = mpegVer
                        .SampleRate = sampleRate
                        .ChannelMode = channelMode
                        .FirstFrameHeader = BigEndianBitConverter.ToUInt32(buffer, offset)
                    ElseIf Not mp3Extractor.isVbr AndAlso bitrate <> .BitRate Then
                        mp3Extractor.isVbr = True

                        If Not mp3Extractor.hasVbrHeader Then
                            If mp3Extractor.delayWrite Then
                                mp3Extractor.WriteVbrHeader(True)
                                mp3Extractor.doWriteVbrHeader = True
                                mp3Extractor.delayWrite = False
                            Else
                                mp3Extractor.ls_warnings.Add("Detected VBR too late, cannot add VBR header.")
                            End If
                        End If
                    End If
                End With
            End If
        End Sub

        Private Sub WriteVbrHeader(isPlaceholder As Boolean)
            Dim buffer As Byte() = New Byte(GetFrameLength(FrameInfo.MpegVersion, 64000, FrameInfo.SampleRate, 0)) {}

            If Not isPlaceholder Then
                Dim header As UInteger = FrameInfo.FirstFrameHeader
                Dim dataOffset As Integer = GetFrameDataOffset(FrameInfo.MpegVersion, FrameInfo.ChannelMode)
                header = header And &HFFFE0DFFUI
                header = header Or (If(FrameInfo.MpegVersion = 3, 5, 8)) << 12
                BitHelper.CopyBytes(buffer, 0, BigEndianBitConverter.GetBytes(header))
                BitHelper.CopyBytes(buffer, dataOffset, BigEndianBitConverter.GetBytes(&H58696E67))
                BitHelper.CopyBytes(buffer, dataOffset + 4, BigEndianBitConverter.GetBytes(&H7))
                BitHelper.CopyBytes(buffer, dataOffset + 8, BigEndianBitConverter.GetBytes(frameOffsets.Count))
                BitHelper.CopyBytes(buffer, dataOffset + 12, BigEndianBitConverter.GetBytes(totalFrameLength))

                For i As Int32 = 0 To 99
                    Dim frameIndex As Integer = ((i / 100.0) * Me.frameOffsets.Count)
                    buffer(dataOffset + 16 + i) = (Me.frameOffsets(frameIndex) / Me.totalFrameLength * 256.0)
                Next
            End If

            Me.p_videoStream.Write(buffer, 0, buffer.Length)
        End Sub
#End Region

    End Class


End Namespace