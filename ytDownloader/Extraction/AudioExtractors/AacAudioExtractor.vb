Imports System.IO

Namespace Extraction.AudioExtractors
    Friend Class AacAudioExtractor
        Inherits AudioChannelExtractor

        Private _aacProfile As Integer
        Private _channelConfig As Integer
        Private _sampleRateIndex As Integer

        Public Sub New(path As String)
            MyBase.New(path)
        End Sub

        Public Overrides Sub WriteChunk(chunk As Byte(), timeStamp As UInteger)
            If chunk.Length < 1 Then Return

            If chunk(0) = 0 Then
                ' Header
                If chunk.Length < 3 Then
                    Return
                End If

                Dim bits As ULong = CULng(BigEndianBitConverter.ToUInt16(chunk, 1)) << 48

                _aacProfile = BitHelper.Read(bits, 5) - 1
                _sampleRateIndex = BitHelper.Read(bits, 4)
                _channelConfig = BitHelper.Read(bits, 4)

                If _aacProfile < 0 OrElse _aacProfile > 3 Then
                    Throw New AudioExtractionException("Unsupported AAC profile.")
                End If
                If _sampleRateIndex > 12 Then
                    Throw New AudioExtractionException("Invalid AAC sample rate index.")
                End If
                If _channelConfig > 6 Then
                    Throw New AudioExtractionException("Invalid AAC channel configuration.")
                End If
            Else
                ' Audio data
                Dim dataSize As Integer = chunk.Length - 1
                Dim bits As ULong = 0

                ' Reference: WriteADTSHeader from FAAC's bitstream.c

                BitHelper.Write(bits, 12, &HFFF)
                BitHelper.Write(bits, 1, 0)
                BitHelper.Write(bits, 2, 0)
                BitHelper.Write(bits, 1, 1)
                BitHelper.Write(bits, 2, _aacProfile)
                BitHelper.Write(bits, 4, _sampleRateIndex)
                BitHelper.Write(bits, 1, 0)
                BitHelper.Write(bits, 3, _channelConfig)
                BitHelper.Write(bits, 1, 0)
                BitHelper.Write(bits, 1, 0)
                BitHelper.Write(bits, 1, 0)
                BitHelper.Write(bits, 1, 0)
                BitHelper.Write(bits, 13, 7 + dataSize)
                BitHelper.Write(bits, 11, &H7FF)
                BitHelper.Write(bits, 2, 0)

                OutputStream.Write(BigEndianBitConverter.GetBytes(bits), 1, 7)
                OutputStream.Write(chunk, 1, dataSize)
            End If
        End Sub

    End Class


End Namespace