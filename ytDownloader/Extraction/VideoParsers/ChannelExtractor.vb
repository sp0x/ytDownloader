Imports System.IO
Imports ytDownloader.Extraction.AudioExtractors

Namespace Extraction.VideoParsers
    Public MustInherit Class ChannelExtractor
        Inherits StreamInput
        Implements IChannelExtractor
        Protected Property AudioExtractor As IAudioExtractor
        Protected Property ChanOutputPath As String
        Public Property ExtractedAudio() As Boolean

#Region "Events"
        Public Event ConversionProgressChanged As EventHandler(Of ProgressEventArgs)
        Protected Sub RaiseConversionProgressChanged(sender As Object, e As ProgressEventArgs)
            RaiseEvent ConversionProgressChanged(sender, e)
        End Sub
#End Region

#Region "Constructor"
        ''' <summary>
        ''' Initializes a new instance of the <see cref="ChannelExtractor"/> class.
        ''' </summary>
        ''' <param name="inputPath">The path of the input.</param>
        ''' <param name="outputPath">The path of the output without extension.</param>
        Public Sub New(inputPath As String, outputPath As String)
            MyBase.New(inputPath)
            ChanOutputPath = outputPath
            CustomOffset = 0
        End Sub
#End Region

#Region "Abstract methods"
        Public MustOverride Sub ExtractStreams() Implements IChannelExtractor.ExtractStreams

        ' Public MustOverride Function GetAudioWriter(param As UInt32) As IAudioExtractor Implements IChannelExtractor.GetAudioWriter
#End Region

#Region "Methods"
        ''' <summary>
        ''' ' Find out the apropriate streamwriter, to use for the audio writing.
        ''' </summary>
        ''' <param name="mediaInfo">The media-info, contains the type of the audio stream.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetAudioWriter(mediaInfo As UInteger) As AudioChannelExtractor Implements IChannelExtractor.GetAudioExtractor
            Dim format As UInteger = mediaInfo >> 4
            Select Case format
                Case 14, 2
                    Return New Mp3AudioExtractor(ChanOutputPath)
                Case 10
                    Return New AacAudioExtractor(ChanOutputPath)
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
#End Region

#Region "IDisposable Support"
        Private _disposedValue As Boolean

        Public Sub CloseOutput(ByVal disposing As Boolean) Implements IChannelExtractor.CloseOutput
            If AudioExtractor Is Nothing Then Return
            Dim fpath As String = AudioExtractor.OutputFile ' OutputStream.Name
            If Not AudioExtractor Is Nothing Then
                If Not disposing AndAlso fpath Is Nothing Then
                    Try
                        File.Delete(fpath)
                    Catch
                    End Try
                End If
                AudioExtractor.Dispose()
                AudioExtractor = Nothing
            End If
        End Sub

        Public Shadows Sub Dispose()
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Private Shadows Sub Dispose(disposing As Boolean)
            If disposing Then
                If Not _disposedValue Then
                    CloseOutput(True)
                    MyBase.Dispose()
                    _disposedValue = True
                End If
            End If

        End Sub
#End Region
    End Class
End Namespace