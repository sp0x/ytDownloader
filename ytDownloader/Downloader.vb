Imports System.Net
Imports System.Reflection
Imports ytDownloader.Extraction
Imports ytDownloader.StringExtensions



''' <summary>
''' Provides the base class for the <see cref="AudioDownloader"/> and <see cref="VideoDownloader"/> class.
''' </summary>
Partial Public MustInherit Class Downloader

#Region "Events"
    ''' <summary>
    ''' Occurs when the download finished.
    ''' </summary>
    Public Event DownloadFinished As EventHandler(Of IoFinishedEventArgs)

    ''' <summary>
    ''' Occurs when the download is starts.
    ''' </summary>
    Public Event DownloadStarted As EventHandler
    Public Event DownloadProgressChanged As EventHandler(Of ProgressEventArgs)
    Public Event ExtractionProgressChanged As EventHandler(Of ProgressEventArgs)


    Friend Sub RaiseDownloadFinished(sender As Object, e As IoFinishedEventArgs)
        RaiseEvent DownloadFinished(sender, e)
    End Sub
    Protected Sub RaiseDownloadStarted(sender As Object, e As EventArgs)
        RaiseEvent DownloadStarted(sender, e)
    End Sub
    Protected Sub RaiseDownloadProgressChanged(sender As Object, e As ProgressEventArgs)
        If e.IsReady OrElse IsUpdateReady(e) Then RaiseEvent DownloadProgressChanged(sender, e)
    End Sub
    Protected Sub RaiseExtractionProgressChanged(sender As Object, e As ProgressEventArgs)
        If e.IsReady OrElse IsUpdateReady(e) Then RaiseEvent ExtractionProgressChanged(sender, e)
    End Sub
#End Region

#Region "Props"
    ''' <summary>
    ''' The url for the video/audio you want to download
    ''' </summary>
    Public Property InputUrl As String

    ' ''' <summary>
    ' ''' Gets the number of bytes to download. <c>null</c>, if everything is downloaded.
    ' ''' </summary>
    'Public Property BytesToDownload As Nullable(Of Integer) = Nothing


    ''' <summary>
    ''' Gets the path to save the video/audio.
    ''' </summary>
    Public Property OutputPath As String
  

    Private _doOptions As New DownloadOptions
    Public Property Options As DownloadOptions
        Get
            If _doOptions Is Nothing Then _doOptions = New DownloadOptions
            Return _doOptions
        End Get
        Set(value As DownloadOptions)
            _doOptions = value
        End Set
    End Property

    Public ReadOnly Property CodecAvailable As Boolean
        Get
            Return _vCodec IsNot Nothing
        End Get
    End Property
    Private _vCodec As VideoCodecInfo
    Public CurrentVideo As YtVideo
    ''' <summary>
    ''' Gets the video to download/convert.
    ''' </summary>
    Public Property VideoCodec As VideoCodecInfo
        Get
            If _vCodec Is Nothing Then
                _vCodec = Options.GetCodec(InputUrl, CurrentVideo)
                _bInitialized = True
            End If
            Return _vCodec
        End Get
        Set(value As VideoCodecInfo)
            _vCodec = value
        End Set
    End Property

    Public Property IsPlaylistMember As Boolean
    Private Property _bInitialized As Boolean = False
    Public ReadOnly Property Initialized() As Boolean
        Get
            Return _bInitialized
        End Get
    End Property
    Private Sub SetInitialized(bInited As Boolean)
        _bInitialized = bInited
    End Sub


#End Region

#Region "Construction"
    Public Sub New()
    End Sub
    Sub New(url As String, ops As DownloadOptions, isPlaylist As Boolean)
        InputUrl = url
        IsPlaylistMember = isPlaylist
        ops.CloneTo(Options)
        Options.SetOnlyVideo(ops.OnlyVideo)
    End Sub
#End Region


#Region "Init"

    ''' <summary>
    ''' This should be called, only after the first element from the video list ha been fetched.
    ''' </summary>
    ''' <param name="dldr">The downloader to initialize.</param>
    ''' <remarks></remarks>
    Public Shared Function Initialize(ByRef dldr As Downloader) As Downloader
        If Not dldr.CodecAvailable Then dldr.VideoCodec = dldr.Options.GetCodec(dldr.InputUrl, dldr.CurrentVideo)
        '   dldr = Factory.Create(dldr.VideoCodec, dldr.Options, dldr.IsPlaylistMember)
        If dldr.Options.OnlyVideo Then
            Factory(Of VideoDownloader).SetExtendor(dldr)
        Else
            Factory(Of AudioDownloader).SetExtendor(dldr)
        End If
        CorrectDownloaderPath(dldr)
        dldr.SetInitialized(True)
        Return dldr
    End Function
    'Public Function Initialize() As Downloader
    '    Initialize(Me)
    '    CorrectDownloaderPath(Me)
    '    Return Me
    'End Function

    Private Shared Sub CorrectDownloaderPath(ByRef dldr As Downloader)
        If Not String.IsNullOrEmpty(dldr.OutputPath) Then
            If System.IO.Directory.Exists(dldr.OutputPath) And Not dldr.OutputPath.EndsWith("\") Then
                dldr.OutputPath &= "\"
            End If
        End If

        If dldr.IsPlaylistMember Then
            If Not String.IsNullOrEmpty(dldr.OutputPath) Then
                If Not IO.Directory.Exists(dldr.OutputPath) Then
                    IO.Directory.CreateDirectory(dldr.OutputPath)
                End If
            End If
        End If

        If Not String.IsNullOrEmpty(dldr.OutputPath) And dldr.IsPlaylistMember Then
            If Not dldr.OutputPath.EndsWith("\") Then dldr.OutputPath &= "\"
        End If
        dldr.OutputPath = String.Format("{0}{1}", dldr.OutputPath, dldr.VideoCodec.Title.RemoveIllegalPathCharacters)
        If TypeOf dldr Is AudioDownloader Then
            dldr.OutputPath = IO.Path.ChangeExtension(dldr.OutputPath, dldr.VideoCodec.AudioExtension)
        ElseIf TypeOf dldr Is VideoDownloader Then
            dldr.OutputPath = IO.Path.ChangeExtension(dldr.OutputPath, dldr.VideoCodec.VideoExtension)
        End If
    End Sub

    Public Shared Function CreateEmpty(url As String, ops As DownloadOptions, isPlaylist As Boolean) As Downloader
        Dim dlm As Downloader = New VideoDownloader
        dlm.InputUrl = url
        dlm.IsPlaylistMember = isPlaylist
        dlm.OutputPath = ops.Output
        ops.CloneTo(dlm.Options)
        dlm.Options.SetOnlyVideo(ops.OnlyVideo)
        Return dlm
    End Function
#End Region


#Region "Update filtering"
    Private Property _updateDelta As Double = 0D
    Public Property UpdateInterval As Double = 2D

    Protected Function IsUpdateReady(ByRef pge As ProgressEventArgs) As Boolean
        Dim shouldPrint As Boolean = False
        Dim delta As Double = Math.Abs(pge.ProgressPercentage - _updateDelta)
        If _updateDelta.Equals(0D) Or _updateDelta.Equals(100D) Then
            shouldPrint = True
            _updateDelta = pge.ProgressPercentage
        ElseIf delta > UpdateInterval Then ' get delta 
            shouldPrint = True
            _updateDelta = pge.ProgressPercentage
        End If
        pge.IsReady = shouldPrint
        Return shouldPrint
    End Function
#End Region

#Region "Starting"
    ''' <summary>
    ''' Starts the work of the <see cref="Downloader"/>.
    ''' </summary>
    Protected MustOverride Sub StartDownloading()

    Public Sub Start()
        If Not Initialized Then
            Throw New InvalidOperationException("Downloader not initialized.")
        End If
        StartDownloading()
    End Sub
    Public Overloads Async Sub StartAsync()
        Await Task.Factory.StartNew(AddressOf Start)
    End Sub
    Public Overloads Sub StartThreaded()
        Task.Factory.StartNew(AddressOf Start)
    End Sub

#End Region

#Region "Overrides"
    Public Overrides Function ToString() As String
        Return MyBase.ToString()
    End Function
#End Region

    Public Function GetVideoImageBytes() As Byte()
        Dim url As String = YtVideo.GetVideoCoverUrl(InputUrl)
        Dim wc As New WebClient()
        Return wc.DownloadData(url)
    End Function

#Region "Cloning"

    Public Function Clone() As Downloader
        Dim res As Downloader = Nothing
        CloneMembersTo(res)
        Return res
    End Function

    Public Sub CloneMembersTo(ByRef dldr As Downloader)
        If dldr Is Nothing Then
            Dim initer As ConstructorInfo = Me.GetType.GetConstructor({})
            dldr = initer.Invoke({})
        End If
        dldr.InputUrl = InputUrl
        dldr.OutputPath = OutputPath
        dldr.Options = Options
        dldr.SetInitialized(Initialized)
        dldr.IsPlaylistMember = IsPlaylistMember
        dldr.UpdateInterval = UpdateInterval
        dldr._updateDelta = _updateDelta
    End Sub

#End Region


End Class