Imports System
Imports System.IO
Imports ytDownloader.Extraction.AudioExtractors


Namespace Extraction.VideoParsers
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
    Friend Class FlvFileParser
        Inherits ChannelExtractor

#Region "Variables"
        Friend Const FlvFileTag As Int32 = &H464C5601
#End Region

#Region "Events"
        Public Shadows Event ConversionProgressChanged As EventHandler(Of ProgressEventArgs)
#End Region

#Region "Construction"
        Public Sub New(input As String, output As String)
            MyBase.New(input, output)
        End Sub
#End Region

#Region "Extractors"

        ''' <exception cref="AudioExtractionException">The input file is not an FLV file.</exception>
        Public Overrides Sub ExtractStreams()
            Seek(0)
            If ReadUInt32() <> FlvFileTag Then
                ' not a FLV file
                Throw New AudioExtractionException("Invalid input file. Impossible to extract audio track.")
            End If
            ReadUInt8()
            Dim dataOffset As UInteger = ReadUInt32()
            Seek(dataOffset)
            ReadUInt32()
            While CustomOffset < GetLength()
                If Not ReadTag() Then
                    Exit While
                End If
                If GetLength() - CustomOffset < 4 Then
                    Exit While
                End If

                ReadUInt32()

                Dim progress As Double = (CustomOffset * 1.0 / GetLength()) * 100
                Dim arg As New ProgressEventArgs(progress) With {.Flag = ProgressFlags.Extraction}
                RaiseEvent ConversionProgressChanged(Me, arg)
                RaiseConversionProgressChanged(Me, arg)
            End While
            Me.CloseOutput(False)
        End Sub

        Private Function ReadTag() As Boolean
            If GetLength() - CustomOffset < 11 Then Return False

            ' Read tag header
            Dim tagType As UInteger = ReadUInt8()
            Dim dataSize As UInteger = ReadUInt24()
            Dim timeStamp As UInteger = ReadUInt24()
            timeStamp = timeStamp Or Me.ReadUInt8() << 24
            Me.ReadUInt24()

            ' Read tag data
            If dataSize = 0 Then Return True

            If GetLength() - CustomOffset < dataSize Then
                Return False
            End If

            Dim mediaInfo As UInteger = ReadUInt8()
            dataSize -= 1
            Dim data As Byte() = ReadBytes(dataSize)

            If tagType = &H8 Then
                ' If we have no audio writer, create one
                If AudioExtractor Is Nothing Then
                    AudioExtractor = GetAudioWriter(mediaInfo)
                    ExtractedAudio = AudioExtractor IsNot Nothing
                End If

                If Me.AudioExtractor Is Nothing Then
                    Throw New InvalidOperationException("No supported audio writer found.")
                End If
                Me.AudioExtractor.WriteChunk(data, timeStamp)
            End If

            Return True
        End Function

#End Region

    End Class


End Namespace