Imports System
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Text.Encoding
Imports ytDownloader.URLHelper

Imports ytDownloader.STD

Namespace Extraction
    Friend Class YtSignitureDecoder
        Private Shared rxFnName As New Regex("\.sig\s*\|\|(\w+)\(", RegexOptions.Compiled)
        Private Shared rxFn2PStrInt As New Regex("\(\w+,(?<index>\d+)\)", RegexOptions.Compiled)
        Private Shared fnBody As String = "(?<brace>{([^{}]| ?(brace))*})"
        Private Shared fnReverse As String = "{0}:\bfunction\b\(\w+\)"
        Private Shared fnSubstring As String = "{0}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."
        Private Shared fnSwap As String = "{0}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"
        Private PlayerSrc As String = ""
        Property PlayerVersion As Object

        Public Sub New(html5ver As String)
            PlayerVersion = html5ver
            Dim jsUrl As String = String.Format("http://s.ytimg.com/yts/jsbin/html5player-{0}.js", PlayerVersion)
            PlayerSrc = DldurlTxt(jsUrl, ASCII)
        End Sub

        Public Function DecipherWithVersion(cipher As String) As String
            If String.IsNullOrEmpty(PlayerSrc) Then
                Throw New InvalidOperationException("Player source not found!")
            End If

            'Find "C" in this: var A = B.sig||C (B.s)
            Dim funcName As String = rxFnName.Match(PlayerSrc).Groups(1).Value
            Dim funcPattern As String = String.Format("{0}\(\w+\){1}", funcName, fnBody)
            Dim funcBody As String = Regex.Match(PlayerSrc, funcPattern).Groups("brace").Value
            Dim lines As String() = funcBody.Split(";"c)

            Dim idReverse As String = "", idSlice As String = "", idCharSwap As String = ""
            Dim strFnName As String = ""
            Dim opCodes As String = ""

            For Each line As String In lines.Skip(1).Take(lines.Length - 2)
                If Not String.IsNullOrEmpty(idReverse) AndAlso Not String.IsNullOrEmpty(idSlice) AndAlso Not String.IsNullOrEmpty(idCharSwap) Then
                    Exit For
                End If

                strFnName = jsGetFunctionFromLine(line)

                Dim rxSwap As String = String.Format("", strFnName)

                If Regex.Match(PlayerSrc, sprintf(fnReverse, strFnName)).Success Then idReverse = strFnName
                If Regex.Match(PlayerSrc, sprintf(fnSubstring, strFnName)).Success Then idSlice = strFnName
                If Regex.Match(PlayerSrc, sprintf(fnSwap, strFnName)).Success Then idCharSwap = strFnName

            Next
            opCodes = getDecipherOps(lines, idReverse, idSlice, idCharSwap)
            Return DecipherWithOperations(cipher, opCodes)
        End Function

        Private Shared Function getDecipherOps(dcLines As String(), fnReverse As String, fnSubstr As String, fnSwap As String) As String
            Dim fnName As String = ""
            Dim opcodes As String = ""
            For Each line As String In dcLines.Skip(1).Take(dcLines.Length - 2)
                Dim m As Match = Nothing
                fnName = jsGetFunctionFromLine(line)

                If inlineHelper(m, rxFn2PStrInt.Match(line)).Success AndAlso fnName = fnSwap Then
                    opcodes &= "w" + m.Groups("index").Value + " "
                    Continue For
                End If

                If inlineHelper(m, rxFn2PStrInt.Match(line)).Success AndAlso fnName = fnSubstr Then
                    opcodes &= "s" + m.Groups("index").Value + " "
                    Continue For
                End If

                If fnName = fnReverse Then
                    opcodes &= "r "
                    Continue For
                End If
            Next

            opcodes = opcodes.Trim()
        End Function

        Private Shared Function DecipherWithOperations(cipher As String, operations As String) As String
            Return operations.Split({" "}, StringSplitOptions.RemoveEmptyEntries).Aggregate(cipher, AddressOf ApplyOperation)
        End Function

        Private Shared Function ApplyOperation(cipher As String, op As String) As String
            Select Case op(0)
                Case "r"c
                    Return New String(cipher.ToCharArray().Reverse().ToArray())
                Case "w"c
                    If True Then
                        Return SwapFirstChar(cipher, GetOpIndex(op))
                    End If
                Case "s"c
                    If True Then
                        Return cipher.Substring(GetOpIndex(op))
                    End If

                Case Else
                    Throw New NotImplementedException("Couldn't find cipher operation.")
                    Return Nothing
            End Select
            Return Nothing
        End Function

        Private Shared Function jsGetFunctionFromLine(currentLine As String) As String
            Dim matchFunctionReg As New Regex("\w+\.(?<functionID>\w+)\(")
            Dim rgMatch As Match = matchFunctionReg.Match(currentLine)
            Dim matchedFunction As String = rgMatch.Groups("functionID").Value
            Return matchedFunction
        End Function

        Private Shared Function GetOpIndex(op As String) As Integer
            Dim parsed As String = New Regex(".(\d+)").Match(op).Result("$1")
            Dim index As Integer = Int32.Parse(parsed)
            Return index
        End Function

        Private Shared Function SwapFirstChar(cipher As String, index As Integer) As String
            Dim builder As New StringBuilder(cipher)
            builder(0) = cipher(index)
            builder(index) = cipher(0)
            Return builder.ToString()
        End Function
    End Class

End Namespace