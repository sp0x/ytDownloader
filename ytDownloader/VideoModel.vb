
Imports System.IO
Imports System.Net
Imports ytDownloader.Extraction

Public Class VideoModel
    Inherits YtVideo  
	Public seconds As Integer 
    Public isPlaylist As Boolean 
    Public property TargetLink As String 

	Public Property Options() As VideoTaskOptions
		Get
			Return m_Options
		End Get
		Private Set
			m_Options = Value
		End Set
	End Property
	Private m_Options As VideoTaskOptions

	Public Sub New(idOrUrl As [String])
        MyBase.New(idOrUrl) 
        YtUrlDecoder.PopulateVideoInfo(idOrUrl , Me )
        if(idOrUrl.Contains("http://") Or idOrUrl.Contains("https://")) then TargetLink = idOrUrl 
        Dim vid = Me.Id

        isPlaylist = YtPlaylist.IsPlaylist(idOrUrl)
		Dim infoUrl = [String].Empty
		'TODO: Improve this to avoid injections of arbitrary urls or ids
'		If idOrUrl.Contains("http://") OrElse idOrUrl.Contains("https://") AndAlso idOrUrl.Contains("youtube.com") Then
'			 idOrUrl = YtVideo.GetVideoId(idOrUrl)
'		End If
		infoUrl = [String].Format("http://youtube.com/get_video_info?video_id={0}", vid) 
        
		Dim req = WebRequest.Create(infoUrl)
		Dim res = req.GetResponse()
		Dim sr As New StreamReader(res.GetResponseStream())
		Dim info = sr.ReadToEnd()
		If Not String.IsNullOrEmpty(info) Then
			Dim infoSplits = info.Split("&".ToCharArray()).ToDictionary(
            Function(row) 
			    Dim eqsplit = row.Split("=".ToCharArray())
			    Return eqsplit(0)
            End Function, Function(row) 
			    Dim eqsplit = row.Split("=".ToCharArray())
			    If eqsplit.Length = 1 Then
				    eqsplit = New String() {eqsplit(0), ""}
			    End If
			    Return eqsplit(1)

            End Function)
			If infoSplits.ContainsKey("title") Then
				name = infoSplits("title").urldecode()
			End If
			If infoSplits.ContainsKey("author") Then
				author = infoSplits("author").urldecode()
			End If
			If infoSplits.ContainsKey("timestamp") Then
				Added = infoSplits("timestamp")
			End If
			If infoSplits.ContainsKey("length_seconds") Then
				Int32.TryParse(infoSplits("length_seconds"), seconds)
			End If

		End If
	End Sub
	''' <summary>
	''' Set options for the video that is targeted.
	''' </summary>
	''' <param name="quality"></param>
	''' <param name="format"></param>
	''' <param name="onlyvideo"></param>
	Public Sub setOptions(quality As String, format As String, onlyvideo As Boolean)
		If Me.Options Is Nothing Then
			Options = New VideoTaskOptions(Me.id, Nothing, onlyvideo, quality, format)
		Else
			Options.Format = format
			Options.Quality = quality
			Options.OnlyVideo = onlyvideo
		End If
	End Sub

	Public Sub setQuality(quality As [String])
		If Me.Options Is Nothing Then
			Options = New VideoTaskOptions(Me.id, Nothing, False, quality, Nothing)
		Else
			Me.Options.Quality = quality
		End If
	End Sub
    

	Public Function getCoverUrl(num As Integer) As [String]
		Return String.Format("http://img.youtube.com/vi/{0}/{1}.jpg", getId() , num)
	End Function
    Public function getId()
        Return YtVideo.GetVideoId(Me.id)
    End function
End Class