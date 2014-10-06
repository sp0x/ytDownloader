Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.IO
Imports System.Text.RegularExpressions

Public Module RegexExtensions
    <Extension>
    Public Function MatchGroupValue(rx As Regex, input As String, group As Int32) As String
        Dim rMtc As Match = rx.Match(input)
        If rMtc IsNot Nothing Then
            Return rMtc.Groups(group).Value
        End If
        Return Nothing
    End Function
End Module
Public Module StringExtensions
    <Extension>
    Public Function RemoveIllegalPathCharacters(path As String) As String
        Dim regexSearch As String = New String(System.IO.Path.GetInvalidFileNameChars()) + New String(System.IO.Path.GetInvalidPathChars())
        Dim r As New Regex(String.Format("[{0}]", Regex.Escape(regexSearch)))
        Return r.Replace(path, "")
    End Function
End Module
Public Module IOExtensions
    <Extension> _
    Public Sub CopyTo(ms As MemoryStream, toStream As Stream)
        Dim tmpBuff As Byte() = (ms.ToArray())
        toStream.Write(tmpBuff, 0, tmpBuff.Length)
    End Sub
End Module

Public Module UrlExtensions
    ''' <summary>
    ''' Decodes URLs, which are encoded with the % hex type encoding, and + for spaces.
    ''' </summary>
    ''' <param name="url">The url to decode</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> _
    Public Function URLDecode(url As String) As String
        Dim tmpBuilder As New StringBuilder
        Dim CurChr As Integer = 1
        Do Until CurChr - 1 = Len(url)
            Select Case Mid(url, CurChr, 1)
                Case "+"
                    tmpBuilder.Append(" ")
                Case "%"
                    tmpBuilder.Append(Chr(Val("&h" & _
                                              Mid(url, CurChr + 1, 2))))
                    CurChr += 2
                Case Else
                    tmpBuilder.Append(Mid(url, CurChr, 1))
            End Select

            CurChr = CurChr + 1
        Loop
        URLDecode = tmpBuilder.ToString
    End Function
    <Extension> Public Function URLEncode(StringToEncode As String, Optional UsePlusRatherThanHexForSpace As Boolean = False) As String
        Dim tmpBuilder As New StringBuilder
        Dim CurChr As Integer = 1
        Do Until CurChr - 1 = Len(StringToEncode)
            Select Case Asc(Mid(StringToEncode, CurChr, 1))
                Case 48 To 57, 65 To 90, 97 To 122
                    tmpBuilder.Append(Mid(StringToEncode, CurChr, 1))
                Case 32
                    If UsePlusRatherThanHexForSpace Then
                        tmpBuilder.Append("+")
                    Else
                        tmpBuilder.Append("%" & Hex(32))
                    End If
                Case Else
                    tmpBuilder.Append("%" & _
                                      Format(Hex(Asc(Mid(StringToEncode, CurChr, 1))), "00"))
            End Select
            CurChr += 1
        Loop
        URLEncode = tmpBuilder.ToString
    End Function


End Module