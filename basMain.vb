Module basMain
	Private WebServerThread As Thread
	
	Function Main() As Integer
		WebServerThread = New Thread(AddressOf ClientConnection.RunServer) With {.IsBackground = True}
		WebServerThread.Start()
		Console.WriteLine("Нажми Enter для закрытия")
		Console.ReadLine()
		Return 0
	End Function

End Module
