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
            'PlayerSrc = DldurlTxt(jsUrl, ASCII)
        End Sub
        

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
            Return opcodes
        End Function

   

        Private Shared Function jsGetFunctionFromLine(currentLine As String) As String
            Dim matchFunctionReg As New Regex("\w+\.(?<functionID>\w+)\(")
            Dim rgMatch As Match = matchFunctionReg.Match(currentLine)
            Dim matchedFunction As String = rgMatch.Groups("functionID").Value
            Return matchedFunction
        End Function
        





        Public Shared Function DecipherWithVersion(cipher As String, cipherVersion As String) As String
		    Dim jsUrl As String = String.Format("http://s.ytimg.com/yts/jsbin/player-{0}.js", cipherVersion)
		    Dim js As String = DldUrlTxt(jsUrl, UTF8)
            '"http://s.ytimg.com/yts/jsbin/html5player-{0}.js", PlayerVersion
		    'Find "C" in this: var A = B.sig||C (B.s)
		    Dim functNamePattern As String = "\.sig\s*\|\|([a-zA-Z0-9\$]+)\("
		    'Regex Formed To Find Word or DollarSign
		    Dim funcName = Regex.Match(js, functNamePattern).Groups(1).Value

		    If funcName.Contains("$") Then
				    'Due To Dollar Sign Introduction, Need To Escape
			    funcName = "\" + funcName
		    End If

		    Dim funcPattern As String = funcName + "=function\(\w+\)\{.*?\}," '; //Escape funcName string
		    Dim funcBody = Regex.Match(js, funcPattern, RegexOptions.Singleline).Value '' //Entire sig function
		    'Entire sig function
		    Dim lines = funcBody.Split(";"C) 

		    'Each line in sig function
		    Dim idReverse As String = "", idSlice As String = "", idCharSwap As String = ""
		    'Hold name for each cipher method
		    Dim functionIdentifier As String = ""
		    Dim operations As String = ""



		    For Each line As String In lines.Skip(1).Take(lines.Length - 2)
			    'Matches the funcBody with each cipher method. Only runs till all three are defined.
			    If Not String.IsNullOrEmpty(idReverse) AndAlso Not String.IsNullOrEmpty(idSlice) AndAlso Not String.IsNullOrEmpty(idCharSwap) Then
					    'Break loop if all three cipher methods are defined
				    Exit For
			    End If

			    functionIdentifier = GetFunctionFromLine(line)
			    Dim reReverse As String = String.Format("{0}:\bfunction\b\(\w+\)", functionIdentifier)
			    'Regex for reverse (one parameter)
			    Dim reSlice As String = String.Format("{0}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.", functionIdentifier)
			    'Regex for slice (return or not)
			    Dim reSwap As String = String.Format("{0}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b", functionIdentifier)
			    'Regex for the char swap.
			    If Regex.Match(js, reReverse).Success Then
					    'If def matched the regex for reverse then the current function is a defined as the reverse
				    idReverse = functionIdentifier
			    End If

			    If Regex.Match(js, reSlice).Success Then
					    'If def matched the regex for slice then the current function is defined as the slice.
				    idSlice = functionIdentifier
			    End If

			    If Regex.Match(js, reSwap).Success Then
					    'If def matched the regex for charSwap then the current function is defined as swap.
				    idCharSwap = functionIdentifier
			    End If
		    Next
            For Each line As String In lines.Skip(1).Take(lines.Length - 2)
			    Dim m As Match
			    functionIdentifier = GetFunctionFromLine(line)

			    If (InlineAssignHelper(m, Regex.Match(line, "\(\w+,(?<index>\d+)\)"))).Success AndAlso functionIdentifier = idCharSwap Then
					    'operation is a swap (w)
				    operations += "w" + m.Groups("index").Value + " "
			    End If

			    If (InlineAssignHelper(m, Regex.Match(line, "\(\w+,(?<index>\d+)\)"))).Success AndAlso functionIdentifier = idSlice Then
					    'operation is a slice
				    operations += "s" + m.Groups("index").Value + " "
			    End If

			    If functionIdentifier = idReverse Then
				    'No regex required for reverse (reverse method has no parameters)
					    'operation is a reverse
				    operations += "r "
			    End If
		    Next

		    operations = operations.Trim()

		    Return DecipherWithOperations(cipher, operations)
	End Function

           
	Private Shared Function ApplyOperation(cipher As String, op As String) As String
		Select Case op(0)
			Case "r"C
				Return New String(cipher.ToCharArray().Reverse().ToArray())

			Case "w"C
				If True Then
					Dim index As Integer = GetOpIndex(op)
					Return SwapFirstChar(cipher, index)
				End If

			Case "s"C
				If True Then
					Dim index As Integer = GetOpIndex(op)
					Return cipher.Substring(index)
				End If
			Case Else

				Throw New NotImplementedException("Couldn't find cipher operation.")
		End Select
	End Function

	Private Shared Function DecipherWithOperations(cipher As String, operations As String) As String
		Return operations.Split(" ".ToCharArray() , StringSplitOptions.RemoveEmptyEntries).Aggregate(cipher, AddressOf ApplyOperation)
	End Function

	Private Shared Function GetFunctionFromLine(currentLine As String) As String
		Dim matchFunctionReg As New Regex("\w+\.(?<functionID>\w+)\(")
		'lc.ac(b,c) want the ac part.
		Dim rgMatch As Match = matchFunctionReg.Match(currentLine)
		Dim matchedFunction As String = rgMatch.Groups("functionID").Value
		Return matchedFunction
		'return 'ac'
	End Function

	Private Shared Function GetOpIndex(op As String) As Integer
		Dim parsed As String = New Regex(".(\d+)").Match(op).Result("$1")
		Dim index As Integer = Int32.Parse(parsed)
		Return index
	End Function
         

	Private Shared Function SwapFirstChar(cipher As String, index As Integer) As String
		Dim builder = New StringBuilder(cipher)
		builder(0) = cipher(index)
		builder(index) = cipher(0)
		Return builder.ToString()
	End Function 

	Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
		target = value
		Return value
	End Function

    End Class

End Namespace