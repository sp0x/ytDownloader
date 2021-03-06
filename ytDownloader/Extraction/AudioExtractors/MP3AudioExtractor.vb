﻿Imports System.Collections.Generic
Imports System.IO
Imports ytDownloader.Extraction.VideoParsers
Imports NAudio.Wave



Namespace Extraction.AudioExtractors
    Public Class MP3Cutter
        Implements IDisposable
        Private Property stream As FileStream
        Private Property reader As Mp3FileReader
        Private Property disposed As Boolean = False
        Public Property OutputDirectory As String
        Public Sub New(mp3File As String, outputDirectory As String)
            stream = New FileStream(mp3File, FileMode.Open, FileAccess.Read, FileShare.Read)
            Try
                reader = New Mp3FileReader(stream)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

            Me.OutputDirectory = outputDirectory
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dictNameOffsetTime"></param>
        ''' <returns></returns>
        Public Function applyTimeDictionary(dictNameOffsetTime As Dictionary(Of String, String))
            If String.IsNullOrEmpty(OutputDirectory) Then
                OutputDirectory = Reflection.Assembly.GetEntryAssembly.Location
                OutputDirectory = IO.Path.GetDirectoryName(OutputDirectory)
            End If
            Dim frame As NAudio.Wave.Mp3Frame = reader.ReadNextFrame()
            Dim lastItem = False
            For iChunk As Int16 = 0 To dictNameOffsetTime.Keys.Count - 1
                Dim key As String = dictNameOffsetTime.Keys(iChunk)
                If key Is Nothing Then Continue For
                Dim value As String = dictNameOffsetTime(dictNameOffsetTime.Keys(iChunk))
                Dim chunk = New KeyValuePair(Of String, String)(key, value)

                If iChunk = (dictNameOffsetTime.Keys.Count - 1) Then
                    lastItem = True
                End If
                Dim timeSplit As String() = Nothing
                If dictNameOffsetTime.Keys.Count = 1 Then
                    Continue For
                Else
                    If Not lastItem Then
                        Dim nextKey = dictNameOffsetTime.Keys(iChunk + 1)
                        If nextKey Is Nothing Then
                            Continue For
                        End If
                        Dim nxChunk = New KeyValuePair(Of String, String)(nextKey, dictNameOffsetTime(dictNameOffsetTime.Keys(iChunk + 1)))
                        timeSplit = nxChunk.Value.Split(":")
                    End If

                End If


                Dim timeOffset = New TimeSpan
                If Not lastItem Then timeOffset = New TimeSpan(CInt(If(timeSplit.Length > 2, timeSplit(0), 0)),
                                              CInt(timeSplit(If(timeSplit.Length > 2, 1, 0))),
                                              CInt(timeSplit.Last().ToString()))

                Dim filename As String = Path.ChangeExtension(chunk.Key, "mp3")
                Try
                    Dim outputFileStream As FileStream = File.Create(Path.Combine(OutputDirectory, filename))

                    While frame IsNot Nothing
                        If (reader.CurrentTime.TotalSeconds >= timeOffset.TotalSeconds) And Not lastItem Then
                            Console.WriteLine(String.Format("Cut {0} at {1} - {2} ", timeOffset.ToString(), reader.CurrentTime.ToString(), filename))
                            'We need to switch to the next file
                            If outputFileStream IsNot Nothing Then outputFileStream.Close()
                            Continue For
                        Else
                            outputFileStream.Write(frame.RawData, 0, frame.RawData.Length)
                            frame = reader.ReadNextFrame()
                        End If

                    End While
                    If lastItem Then
                        If outputFileStream IsNot Nothing Then outputFileStream.Close()
                    End If

                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                    Return False
                End Try

            Next
            Return True
        End Function


        Public Sub Dispose() Implements IDisposable.Dispose
            If disposed Then Return
            If reader IsNot Nothing And stream IsNot Nothing Then
                Try
                    stream.Close()
                    reader.Dispose()
                Catch ex As Exception

                End Try
            End If
        End Sub
    End Class
    Friend Class Mp3AudioExtractor
        Inherits AudioChannelExtractor

#Region "Static variables"
        Shared ReadOnly Mpeg1BitRate As Integer() = New Int32() {0, 32, 40, 48, 56, 64,
             80, 96, 112, 128, 160, 192,
             224, 256, 320, 0}
        Shared ReadOnly Mpeg2XBitRate As Integer() = New Int32() {0, 8, 16, 24, 32, 40,
                48, 56, 64, 80, 96, 112,
                128, 144, 160, 0}
        Shared ReadOnly Mpeg1SampleRate As Integer() = New Int32() {44100, 48000, 32000, 0}
        Shared ReadOnly Mpeg20SampleRate As Integer() = New Int32() {22050, 24000, 16000, 0}
        Shared ReadOnly Mpeg25SampleRate As Integer() = New Int32() {11025, 12000, 8000, 0}
#End Region

#Region "Variables"
        Private frameOffsets As List(Of UInteger)
        Private ls_warnings As List(Of String)
        Private _hasVbrHeader As Boolean
        Private _isVbr As Boolean
        Private FrameInfo As New Mp3BasicFrame
        Private _totalFrameLength As UInteger
        Private _doWriteVbrHeader As Boolean
        Private _delayWrite As Boolean
#End Region

#Region "Construction"
        Public Sub New(path As String)
            MyBase.New(path)
            Me.ls_warnings = New List(Of String)()
            Me.frameOffsets = New List(Of UInteger)()
            Me._delayWrite = True
        End Sub
#End Region

#Region "Props"
        Public ReadOnly Property Warnings() As IEnumerable(Of String)
            Get
                Return Me.ls_warnings
            End Get
        End Property
#End Region

#Region "Frame writer"
        ''' <summary>
        ''' Writes the FLV Buffer, by extracting it to a MP3 Frame.
        ''' </summary>
        ''' <param name="chunk">The buffer to write</param>
        ''' <param name="timeStamp">Not used</param>
        ''' <remarks></remarks>
        Public Overrides Sub WriteChunk(chunk As Byte(), timeStamp As UInteger)
            Buffer.Write(chunk, 0, chunk.Length)
            ParseMp3Frame(chunk)
            If Me._delayWrite AndAlso Me._totalFrameLength >= 65536 Then
                Me._delayWrite = False
            End If
            If Not _delayWrite Then
                Flush()
            End If
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
        Private Sub getMp3FrameInfo(ByRef header As ULong, ByRef mpegVersion As Int32, ByRef layer As Int32, ByRef bitrate As Int32, ByRef padding As Int32,
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
                    sampleRate = Mpeg20SampleRate(sampleRate)
                Case 3
                    sampleRate = Mpeg1SampleRate(sampleRate)
                Case Else
                    sampleRate = Mpeg25SampleRate(sampleRate)
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
                    mp3Extractor._delayWrite = False
                    mp3Extractor._hasVbrHeader = True
                End If
            End If
            Return isVbrHeaderFrame
        End Function
#End Region

#Region "Parsing"
        Private Sub ParseMp3Frame(bFrame As Byte())
            Dim offset As Integer = 0
            Dim length As Integer = bFrame.Length

            While length >= 4
                Dim mpegVersion, sampleRate, channelMode As Integer
                Dim layer, bitRate, padding As Integer
                Dim header As ULong = CULng(BigEndianBitConverter.ToUInt32(bFrame, offset)) << 32

                If BitHelper.Read(header, 11) <> &H7FF Then
                    Exit While
                End If

                getMp3FrameInfo(header, mpegVersion, layer, bitRate, padding, channelMode, sampleRate)

                If mpegVersion = 1 OrElse layer <> 1 OrElse bitRate = 0 OrElse bitRate = 15 OrElse sampleRate = 3 Then
                    Exit While
                End If

                bitRate = (If(mpegVersion = 3, Mpeg1BitRate(bitRate), Mpeg2XBitRate(bitRate))) * 1000
                calcSampleRate(mpegVersion, sampleRate)
                Dim frameLenght As Integer = GetFrameLength(mpegVersion, bitRate, sampleRate, padding)
                If frameLenght > length Then
                    Exit While
                End If

                ParseHeaderInformation(Me, offset, bFrame, bitRate, mpegVersion, sampleRate, channelMode)


                Me.frameOffsets.Add(Me._totalFrameLength + offset)
                offset += frameLenght
                length -= frameLenght
            End While

            Me._totalFrameLength += bFrame.Length
        End Sub

        Private Shared Sub ParseHeaderInformation(ByRef mp3Extractor As Mp3AudioExtractor,
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
                    ElseIf Not mp3Extractor._isVbr AndAlso bitrate <> .BitRate Then
                        mp3Extractor._isVbr = True

                        If Not mp3Extractor._hasVbrHeader Then
                            If mp3Extractor._delayWrite Then
                                mp3Extractor.WriteVbrHeader(True)
                                mp3Extractor._doWriteVbrHeader = True
                                mp3Extractor._delayWrite = False
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
                BitHelper.CopyBytes(buffer, dataOffset + 12, BigEndianBitConverter.GetBytes(_totalFrameLength))

                For i As Int32 = 0 To 99
                    Dim frameIndex As Integer = ((i / 100.0) * Me.frameOffsets.Count)
                    buffer(dataOffset + 16 + i) = (Me.frameOffsets(frameIndex) / Me._totalFrameLength * 256.0)
                Next
            End If

            Output.Write(buffer, 0, buffer.Length)
        End Sub
#End Region

#Region "Disposition"
        Public Overloads Sub Dispose()
            MyBase.Flush()
            If Me._doWriteVbrHeader Then
                Output.Seek(0, SeekOrigin.Begin)
                WriteVbrHeader(False)
            End If
            MyBase.Dispose()
        End Sub
#End Region

    End Class
End Namespace