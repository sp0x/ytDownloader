Imports System
Imports System.IO


Namespace Extraction
    ' ****************************************************************************
    '
    ' FLV Extract
    ' Copyright (C) 2006-2012  J.D. Purcell (moitah@yahoo.com)
    '
    ' This program is free software; you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation; either version 2 of the License, or
    ' (at your option) any later version.
    '
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    '
    ' You should have received a copy of the GNU General Public License
    ' along with this program; if not, write to the Free Software
    ' Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
    '
    ' ****************************************************************************
    Friend Class FlvFile
        Implements IDisposable
#Region "Variables"
        Private ReadOnly fileLength As Long
        Private ReadOnly inputPath As String
        Private ReadOnly outputPath As String
        Private audioExtractor As IAudioExtractor
        Private fileOffset As Long
        Private fileStream As FileStream
        Public Event ConversionProgressChanged As EventHandler(Of ProgressEventArgs)
        Public Property ExtractedAudio() As Boolean
#End Region

#Region "Construction"
        ''' <summary>
        ''' Initializes a new instance of the <see cref="FlvFile"/> class.
        ''' </summary>
        ''' <param name="inputPath">The path of the input.</param>
        ''' <param name="outputPath">The path of the output without extension.</param>
        Public Sub New(inputPath As String, outputPath As String)
            Me.inputPath = inputPath
            Me.outputPath = outputPath
            Me.fileStream = New FileStream(Me.inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024)
            Me.fileOffset = 0
            Me.fileLength = fileStream.Length
        End Sub
#End Region

#Region "Disposition"
        Public Sub Dispose() Implements IDisposable.Dispose
            Me.Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Private Sub Dispose(disposing As Boolean)
            If disposing Then
                If Not Me.fileStream Is Nothing Then
                    Me.fileStream.Close()
                    Me.fileStream = Nothing
                End If

                Me.CloseOutput(True)
            End If
        End Sub
        Private Sub CloseOutput(disposing As Boolean)
            If audioExtractor Is Nothing Then Return
            Dim fpath As String = audioExtractor.VideoStream.Name
            If Not Me.audioExtractor Is Nothing Then
                If Not disposing AndAlso fpath Is Nothing Then
                    Try
                        File.Delete(fpath)
                    Catch
                    End Try
                End If

                Me.audioExtractor.Dispose()
                Me.audioExtractor = Nothing
            End If
        End Sub
#End Region


        Friend Const FLV_FILE_TAG As Int32 = &H464C5601

        ''' <exception cref="AudioExtractionException">The input file is not an FLV file.</exception>
        Public Sub ExtractStreams()
            Me.Seek(0)
            If Me.ReadUInt32() <> FLV_FILE_TAG Then
                ' not a FLV file
                Throw New AudioExtractionException("Invalid input file. Impossible to extract audio track.")
            End If
            Me.ReadUInt8()
            Dim dataOffset As UInteger = Me.ReadUInt32()
            Me.Seek(dataOffset)
            Me.ReadUInt32()
            While fileOffset < fileLength
                If Not ReadTag() Then
                    Exit While
                End If
                If fileLength - fileOffset < 4 Then
                    Exit While
                End If

                Me.ReadUInt32()

                Dim progress As Double = (Me.fileOffset * 1.0 / Me.fileLength) * 100
                RaiseEvent ConversionProgressChanged(Me, New ProgressEventArgs(progress))
            End While

            Me.CloseOutput(False)
        End Sub

        ''' <summary>
        ''' ' Find out the apropriate streamwriter, to use for the audio writing.
        ''' </summary>
        ''' <param name="mediaInfo">The media-info, contains the type of the audio stream.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetAudioWriter(mediaInfo As UInteger) As IAudioExtractor
            Dim format As UInteger = mediaInfo >> 4

            Select Case format
                Case 14, 2
                    Return New Mp3AudioExtractor(Me.outputPath)
                Case 10
                    Return New AacAudioExtractor(Me.outputPath)
            End Select

            Dim typeStr As String
            Select Case format
                Case 1
                    typeStr = "ADPCM"
                Case 6, 5, 4
                    typeStr = "Nellymoser"
                Case Else
                    typeStr = "format=" + format
            End Select
            Throw New AudioExtractionException("Unable to extract audio (" + typeStr + " is unsupported).")
        End Function

        Private Function ReadTag() As Boolean
            If Me.fileLength - Me.fileOffset < 11 Then  Return False
        
            ' Read tag header
            Dim tagType As UInteger = ReadUInt8()
            Dim dataSize As UInteger = ReadUInt24()
            Dim timeStamp As UInteger = ReadUInt24()
            timeStamp = timeStamp Or Me.ReadUInt8() << 24
            Me.ReadUInt24()

            ' Read tag data
            If dataSize = 0 Then Return True

            If Me.fileLength - Me.fileOffset < dataSize Then
                Return False
            End If

            Dim mediaInfo As UInteger = Me.ReadUInt8()
            dataSize -= 1
            Dim data As Byte() = Me.ReadBytes(dataSize)

            If tagType = &H8 Then
                ' If we have no audio writer, create one
                If Me.audioExtractor Is Nothing Then
                    Me.audioExtractor = Me.GetAudioWriter(mediaInfo)
                    Me.ExtractedAudio = Me.audioExtractor IsNot Nothing
                End If

                If Me.audioExtractor Is Nothing Then
                    Throw New InvalidOperationException("No supported audio writer found.")
                End If
                Me.audioExtractor.WriteChunk(data, timeStamp)
            End If

            Return True
        End Function

#Region "Readers"
        Private Function ReadBytes(length As Integer) As Byte()
            Dim buff As Byte() = New Byte(length - 1) {}

            Me.fileStream.Read(buff, 0, length)
            Me.fileOffset += length

            Return buff
        End Function

        Private Function ReadUInt24() As UInteger
            Dim x As Byte() = New Byte(4) {}

            Me.fileStream.Read(x, 1, 3)
            Me.fileOffset += 3

            Return BigEndianBitConverter.ToUInt32(x, 0)
        End Function

        Private Function ReadUInt32() As UInteger
            Dim x As Byte() = New Byte(4) {}

            Me.fileStream.Read(x, 0, 4)
            Me.fileOffset += 4

            Return BigEndianBitConverter.ToUInt32(x, 0)
        End Function

        Private Function ReadUInt8() As UInteger
            Me.fileOffset += 1
            Return Me.fileStream.ReadByte()
        End Function
        Private Sub Seek(offset As Long)
            Me.fileStream.Seek(offset, SeekOrigin.Begin)
            Me.fileOffset = offset
        End Sub
#End Region


    End Class


End Namespace