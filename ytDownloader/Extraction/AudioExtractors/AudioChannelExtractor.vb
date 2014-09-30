Imports System.IO
Imports ytDownloader.Extraction.VideoParsers

Namespace Extraction.AudioExtractors
    Public MustInherit Class AudioChannelExtractor
        Inherits StreamOutput
        Implements IAudioExtractor

#Region "Props"
        Public Property OutputFile As String Implements IAudioExtractor.OutputFile
        Public Property OutputStream As Stream Implements IAudioExtractor.OutputStream
            Get
                Return MyBase.Output
            End Get
            Set(value As Stream)
                MyBase.Output = value
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New()
            MyBase.New()
        End Sub
        ''' <summary>
        ''' Creates a new Audio channel extractor, binded to the given output path
        ''' </summary>
        ''' <param name="outputPath"></param>
        ''' <remarks></remarks>
        Public Sub New(outputPath As String)
            MyBase.New(outputPath)
            OutputFile = outputPath
        End Sub
        Public Sub New(outputStream As Stream)
            MyBase.New(outputStream)
         End Sub
#End Region

#Region "Abstract methods"
        Public MustOverride Sub WriteChunk(chunk() As Byte, timeStamp As UInteger) Implements IAudioExtractor.WriteChunk
#End Region


#Region "IDisposable Support"
        Private _disposedValue As Boolean ' To detect redundant calls

        Protected Overloads Sub Dispose(disposing As Boolean)
            If Not Me._disposedValue Then
                If disposing Then
                    MyBase.Dispose()
                End If
            End If
            Me._disposedValue = True
        End Sub
        Public Overloads Sub Dispose()
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End NameSpace