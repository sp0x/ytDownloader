Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports ytDownloader.Extraction

Friend Class URLHelper
    Public Shared Function Dldurl(ByVal url As String, Optional ByRef user As String = "", Optional ByRef pass As String = "") As Byte()
        Dim h As System.Net.HttpWebRequest = System.Net.HttpWebRequest.Create(url)
        h.Method = "GET"
        Dim ms As New System.IO.MemoryStream
        If Not String.IsNullOrEmpty(user) And Not String.IsNullOrEmpty(pass) Then h.Credentials = New NetworkCredential(user, pass)
        Dim str As System.IO.Stream = h.GetResponse.GetResponseStream
        Dim byts() As Byte = New Byte(1024 * 8) {}
        str.ReadTimeout = 5000
        Do : Dim rdTmp As Int32 = 0
            Try : rdTmp = str.Read(byts, 0, byts.Length) : Catch : End Try : If rdTmp = 0 Then Exit Do
            ms.Write(byts, 0, rdTmp)
        Loop : Return ms.ToArray
    End Function
    Public Shared Function DldurlTxt(ByVal url As String, ByVal encoder As Encoding, _
                                     Optional ByVal user As String = "", Optional ByVal pass As String = "") As String
        If String.IsNullOrEmpty(url) Then Return ""
        Dim byts() As Byte = Dldurl(url, user, pass)
        If encoder Is Nothing Then encoder = ASCIIEncoding.Unicode
        Return encoder.GetString(byts)
    End Function

    Public Shared Function LinkSetArg(link As String, argument As String, setTo As String) As String
        If link.Substring(link.IndexOf(argument) + argument.Length + 1).StartsWith("&") Then
            link = link.Replace(argument & "=", argument & "=" & SetTo)
        ElseIf (link.Substring(link.IndexOf(argument) + argument.Length + 1).StartsWith("&")) = False Then
            If Not link.Contains(argument & "=") Then
                link = LinkAddArg(link, argument, setTo)
            Else
                link = link.Replace(Split(link.Substring(link.IndexOf(argument & "=")), "&")(0), argument & "=" & setTo)
            End If
           End If
        Return link
    End Function
    Public Shared Function LinkAddArg(url As String, arg As String, value As String) As String
        Dim argCouple As String = String.Format("{0}={1}", arg, value)
        If Not url.Contains("?") Then
            url &= "?"
            url &= argCouple
        Else
            url = String.Format("{0}{1}{2}", url, If(Not url.EndsWith("&"), "&", ""), argCouple)
        End If
        Return url
    End Function



    Public Shared Function ParseQueryString(s As String) As IDictionary(Of String, String)
        ' remove anything other than query string from url
        If s.Contains("?") Then
            s = s.Substring(s.IndexOf("?"c) + 1)
        End If

        Dim dictionary As New Dictionary(Of String, String)()

        For Each vp As String In Regex.Split(s, "&")
            Dim strings As String() = Regex.Split(vp, "=")
            dictionary.Add(strings(0), If(strings.Length = 2, HtmlDecoder.Decode(strings(1)), String.Empty))
        Next

        Return dictionary
    End Function

    Public Shared Function ReplaceQueryStringParameter(currentPageUrl As String, paramToReplace As String, newValue As String) As String
        Dim query = ParseQueryString(currentPageUrl)

        query(paramToReplace) = newValue

        Dim resultQuery As New StringBuilder()
        Dim isFirst As Boolean = True

        For Each pair As KeyValuePair(Of String, String) In query
            If Not isFirst Then
                resultQuery.Append("&")
            End If

            resultQuery.Append(pair.Key)
            resultQuery.Append("=")
            resultQuery.Append(pair.Value)

            isFirst = False
        Next

        Dim uriBuilder As New UriBuilder(currentPageUrl) With { _
           .Query = resultQuery.ToString() _
        }

        Return uriBuilder.ToString()
    End Function

    Private Shared Function WResponseToString(response As WebResponse) As String
        Using responseStream As Stream = response.GetResponseStream()
            Using sr As New StreamReader(responseStream)
                Return sr.ReadToEnd()
            End Using
        End Using
    End Function
End Class

