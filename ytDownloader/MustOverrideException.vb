
Class MustOverrideException
    Inherits Exception

    Private [MustOverride] As String

    Sub New(p1 As String)
        MyBase.New()
        [MustOverride] = p1
    End Sub

End Class
