''' <summary>
''' Сайт на сервере
''' </summary>
<Serializable()> Public Class WebSite

	''' <summary>
	''' Имя сайта
	''' </summary>
	Public HostName As String

	''' <summary>
	''' Физический путь
	''' </summary>
	Public PhysicalDirectory As String

	''' <summary>
	''' Виртуальный путь
	''' </summary>
	Public VirtualPath As String

	''' <summary>
	''' Сертификат сайта
	''' </summary>
	Public ServerCertificate As X509Certificate

	''' <summary>
	''' Сайт перемещён на другой ресурс
	''' </summary>
	Public IsMoved As Boolean

	''' <summary>
	''' Имя серурса, куда перемещён сайт
	''' </summary>
	Public MovedUrl As String

End Class
