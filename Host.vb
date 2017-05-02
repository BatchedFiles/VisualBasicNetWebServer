<Serializable()> Public Class Host
	Inherits MarshalByRefObject
	Implements IRegisteredObject

	Public Sub ProcessRequest(ByVal state As ReadHeadersResult, ByVal objStream As Stream, ByVal VirtualPath As String, ByVal PhysicalPath As String)
		Dim r As New Request(state, objStream, VirtualPath, PhysicalPath)
		HttpRuntime.ProcessRequest(r)
	End Sub

	Public Sub [Stop](ByVal immediate As Boolean) Implements System.Web.Hosting.IRegisteredObject.Stop
		HostingEnvironment.UnregisterObject(Me)
	End Sub

	Public Overloads Overrides Function InitializeLifetimeService() As Object
		Return Nothing
	End Function

	Public Sub New()
		HostingEnvironment.RegisterObject(Me)
	End Sub

	Public Sub Shutdown()
		HostingEnvironment.InitiateShutdown()
	End Sub
End Class
