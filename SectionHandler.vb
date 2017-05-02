Public Class WebServerSectionGroup
	Inherits ConfigurationSectionGroup

	<ConfigurationProperty("contentTypes", IsDefaultCollection:=False),
	 ConfigurationCollection(GetType(ContentTypesSection),
	 AddItemName:="add", ClearItemsName:="clear", RemoveItemName:="remove")> _
	Public ReadOnly Property ContentTypes() As ContentTypesSection
		Get
			Return CType(MyBase.Sections.Item("contentTypes"), ContentTypesSection)
		End Get
	End Property

	<ConfigurationProperty("aspNetProcessingFiles", IsDefaultCollection:=False),
	 ConfigurationCollection(GetType(AspNetProcessingFilesSection),
	 AddItemName:="add", ClearItemsName:="clear", RemoveItemName:="remove")> _
	Public ReadOnly Property AspNetProcessingFiles() As AspNetProcessingFilesSection
		Get
			Return CType(MyBase.Sections.Item("aspNetProcessingFiles"), AspNetProcessingFilesSection)
		End Get
	End Property

	<ConfigurationProperty("webSites", IsDefaultCollection:=False),
	ConfigurationCollection(GetType(WebSitesSection),
	AddItemName:="add", ClearItemsName:="clear", RemoveItemName:="remove")> _
	Public ReadOnly Property WebSites() As WebSitesSection
		Get
			Return CType(MyBase.Sections.Item("webSites"), WebSitesSection)
		End Get
	End Property

End Class

Public Class ContentTypesSection
	Inherits ConfigurationSection

	<ConfigurationProperty("contentTypes", IsDefaultCollection:=False),
	 ConfigurationCollection(GetType(ContentTypeCollection),
	 AddItemName:="add", ClearItemsName:="clear", RemoveItemName:="remove")> _
	Public ReadOnly Property ContentTypes() As ContentTypeCollection
		Get
			Dim urlsCollection As ContentTypeCollection = CType(MyBase.Item("contentTypes"), ContentTypeCollection)
			Return urlsCollection
		End Get
	End Property

End Class

Public Class AspNetProcessingFilesSection
	Inherits ConfigurationSection

	<ConfigurationProperty("aspNetProcessingFiles", IsDefaultCollection:=False),
	 System.Configuration.ConfigurationCollection(GetType(AspNetProcessingFilesCollection),
	 AddItemName:="add", ClearItemsName:="clear", RemoveItemName:="remove")> _
	Public ReadOnly Property AspNetProcessingFiles() As AspNetProcessingFilesCollection
		Get
			Dim urlsCollection As AspNetProcessingFilesCollection = CType(MyBase.Item("aspNetProcessingFiles"), AspNetProcessingFilesCollection)
			Return urlsCollection
		End Get
	End Property

End Class

Public Class WebSitesSection
	Inherits ConfigurationSection

	<ConfigurationProperty("webSites", IsDefaultCollection:=False),
	 System.Configuration.ConfigurationCollection(GetType(WebSitesCollection),
	 AddItemName:="add", ClearItemsName:="clear", RemoveItemName:="remove")> _
	Public ReadOnly Property WebSites() As WebSitesCollection
		Get
			Dim urlsCollection As WebSitesCollection = CType(MyBase.Item("webSites"), WebSitesCollection)
			Return urlsCollection
		End Get
	End Property

End Class

Public Class ContentTypeCollection
	Inherits ConfigurationElementCollection

	Public Sub New()
		Dim ext As ContentTypeElement = CType(CreateNewElement(), ContentTypeElement)
		Add(ext)
	End Sub

	Public Overrides ReadOnly Property CollectionType() As ConfigurationElementCollectionType
		Get
			Return ConfigurationElementCollectionType.AddRemoveClearMap
		End Get
	End Property

	Protected Overloads Overrides Function CreateNewElement() As ConfigurationElement
		Return New ContentTypeElement()
	End Function

	Protected Overrides Function GetElementKey(ByVal element As ConfigurationElement) As Object
		Return (CType(element, ContentTypeElement)).Extension
	End Function

	Default Shadows Property Item(ByVal index As Integer) As ContentTypeElement
		Get
			Return CType(BaseGet(index), ContentTypeElement)
		End Get
		Set(ByVal value As ContentTypeElement)
			If BaseGet(index) IsNot Nothing Then
				BaseRemoveAt(index)
			End If
			BaseAdd(index, value)
		End Set
	End Property

	Default Public Shadows ReadOnly Property Item(ByVal extentios As String) As ContentTypeElement
		Get
			Return CType(BaseGet(extentios), ContentTypeElement)
		End Get
	End Property

	Public Function IndexOf(ByVal url As ContentTypeElement) As Integer
		Return BaseIndexOf(url)
	End Function

	Public Sub Add(ByVal url As ContentTypeElement)
		BaseAdd(url)
	End Sub

	Protected Overloads Overrides Sub BaseAdd(ByVal element As ConfigurationElement)
		BaseAdd(element, False)
	End Sub

	Public Sub Remove(ByVal url As ContentTypeElement)
		If BaseIndexOf(url) >= 0 Then
			BaseRemove(url.Extension)
		End If
	End Sub

	Public Sub RemoveAt(ByVal index As Integer)
		BaseRemoveAt(index)
	End Sub

	Public Sub Remove(ByVal extension As String)
		BaseRemove(extension)
	End Sub

	Public Sub Clear()
		BaseClear()
	End Sub
End Class

<ConfigurationCollection(GetType(AspNetProcessingFilesElement))> _
Public Class AspNetProcessingFilesCollection
	Inherits ConfigurationElementCollection

	Public Sub New()
		Dim ext As AspNetProcessingFilesElement = CType(CreateNewElement(), AspNetProcessingFilesElement)
		Add(ext)
	End Sub

	Public Overrides ReadOnly Property CollectionType() As ConfigurationElementCollectionType
		Get
			Return ConfigurationElementCollectionType.AddRemoveClearMap
		End Get
	End Property

	Protected Overloads Overrides Function CreateNewElement() As ConfigurationElement
		Return New AspNetProcessingFilesElement()
	End Function

	Protected Overrides Function GetElementKey(ByVal element As ConfigurationElement) As Object
		Return (CType(element, AspNetProcessingFilesElement)).Extension
	End Function

	Default Shadows Property Item(ByVal index As Integer) As AspNetProcessingFilesElement
		Get
			Return CType(BaseGet(index), AspNetProcessingFilesElement)
		End Get
		Set(ByVal value As AspNetProcessingFilesElement)
			If BaseGet(index) IsNot Nothing Then
				BaseRemoveAt(index)
			End If
			BaseAdd(index, value)
		End Set
	End Property

	Default Public Shadows ReadOnly Property Item(ByVal extentios As String) As AspNetProcessingFilesElement
		Get
			Return CType(BaseGet(extentios), AspNetProcessingFilesElement)
		End Get
	End Property

	Public Function IndexOf(ByVal url As AspNetProcessingFilesElement) As Integer
		Return BaseIndexOf(url)
	End Function

	Public Sub Add(ByVal url As AspNetProcessingFilesElement)
		BaseAdd(url)
	End Sub

	Protected Overloads Overrides Sub BaseAdd(ByVal element As ConfigurationElement)
		BaseAdd(element, False)
	End Sub

	Public Sub Remove(ByVal url As AspNetProcessingFilesElement)
		If BaseIndexOf(url) >= 0 Then
			BaseRemove(url.Extension)
		End If
	End Sub

	Public Sub RemoveAt(ByVal index As Integer)
		BaseRemoveAt(index)
	End Sub

	Public Sub Remove(ByVal extension As String)
		BaseRemove(extension)
	End Sub

	Public Sub Clear()
		BaseClear()
	End Sub
End Class

<ConfigurationCollection(GetType(WebSitesElement))> _
Public Class WebSitesCollection
	Inherits ConfigurationElementCollection

	Public Sub New()
		Dim wse As WebSitesElement = CType(CreateNewElement(), WebSitesElement)
		Add(wse)
	End Sub

	Public Overrides ReadOnly Property CollectionType() As ConfigurationElementCollectionType
		Get
			Return ConfigurationElementCollectionType.AddRemoveClearMap
		End Get
	End Property

	Protected Overloads Overrides Function CreateNewElement() As ConfigurationElement
		Return New WebSitesElement()
	End Function

	Protected Overrides Function GetElementKey(ByVal element As ConfigurationElement) As Object
		Return (CType(element, WebSitesElement)).HostName
	End Function

	Default Shadows Property Item(ByVal index As Integer) As WebSitesElement
		Get
			Return CType(BaseGet(index), WebSitesElement)
		End Get
		Set(ByVal value As WebSitesElement)
			If BaseGet(index) IsNot Nothing Then
				BaseRemoveAt(index)
			End If
			BaseAdd(index, value)
		End Set
	End Property

	Default Public Shadows ReadOnly Property Item(ByVal siteName As String) As WebSitesElement
		Get
			Return CType(BaseGet(siteName), WebSitesElement)
		End Get
	End Property

	Public Function IndexOf(ByVal url As WebSitesElement) As Integer
		Return BaseIndexOf(url)
	End Function

	Public Sub Add(ByVal url As WebSitesElement)
		BaseAdd(url)
	End Sub

	Protected Overloads Overrides Sub BaseAdd(ByVal element As ConfigurationElement)
		BaseAdd(element, False)
	End Sub

	Public Sub Remove(ByVal url As WebSitesElement)
		If BaseIndexOf(url) >= 0 Then
			BaseRemove(url.HostName)
		End If
	End Sub

	Public Sub RemoveAt(ByVal index As Integer)
		BaseRemoveAt(index)
	End Sub

	Public Sub Remove(ByVal hostName As String)
		BaseRemove(hostName)
	End Sub

	Public Sub Clear()
		BaseClear()
	End Sub
End Class

Public Class ContentTypeElement
	Inherits ConfigurationElement

	Public Sub New()
		MyBase.new()
	End Sub

	Public Sub New(ByVal extension As String, ByVal contentType As String, ByVal isText As Boolean)
		Me.Item("extension") = extension
		Me.Item("contentType") = contentType
		Me.Item("isText") = isText
	End Sub

	<ConfigurationProperty("extension", isrequired:=True, IsKey:=True)> Public Property Extension As String
		Get
			Return Me.Item("extension").ToString
		End Get
		Set(ByVal value As String)
			Me.Item("extension") = value
		End Set
	End Property

	<ConfigurationProperty("contentType", isrequired:=True)> Public Property ContentType As String
		Get
			Return Me.Item("contentType").ToString
		End Get
		Set(ByVal value As String)
			Me.Item("contentType") = value
		End Set
	End Property

	<ConfigurationProperty("isText", isrequired:=True)> Public Property IsTextFormat As Boolean
		Get
			Return Convert.ToBoolean(Me.Item("isText"))
		End Get
		Set(ByVal value As Boolean)
			Me.Item("isText") = value
		End Set
	End Property

End Class

Public Class AspNetProcessingFilesElement
	Inherits ConfigurationElement

	Public Sub New()
		MyBase.new()
	End Sub

	Public Sub New(ByVal extension As String)
		Me.Item("extension") = extension
	End Sub

	<ConfigurationProperty("extension", isrequired:=True, iskey:=True)> Public Property Extension As String
		Get
			Return Me.Item("extension").ToString
		End Get
		Set(ByVal value As String)
			Me.Item("extension") = value
		End Set
	End Property

End Class

Public Class WebSitesElement
	Inherits ConfigurationElement

	Public Sub New()
		MyBase.new()
	End Sub

	Public Sub New(ByVal virtualPath As String, ByVal physicalDirectory As String, ByVal hostName As String, ByVal enableSsl As Boolean, ByVal certificateFilePath As String, ByVal isMoved As Boolean, ByVal movedUrl As String)
		Me.Item("virtualPath") = virtualPath
		Me.Item("physicalDirectory") = physicalDirectory
		Me.Item("hostName") = hostName
		Me.Item("enableSsl") = enableSsl
		Me.Item("certificateFilePath") = certificateFilePath
		Me.Item("isMoved") = isMoved
		Me.Item("movedUrl") = movedUrl
	End Sub

	<ConfigurationProperty("certificateFilePath", isrequired:=True, iskey:=False)> Public Property CertificateFilePath As String
		Get
			Return Me.Item("certificateFilePath").ToString
		End Get
		Set(ByVal value As String)
			Me.Item("certificateFilePath") = value
		End Set
	End Property

	<ConfigurationProperty("physicalDirectory", isrequired:=True, iskey:=False)> Public Property PhysicalDirectory As String
		Get
			Return Me.Item("physicalDirectory").ToString
		End Get
		Set(ByVal value As String)
			Me.Item("physicalDirectory") = value
		End Set
	End Property

	<ConfigurationProperty("hostName", isrequired:=True, iskey:=True)> Public Property HostName As String
		Get
			Return Me.Item("hostName").ToString
		End Get
		Set(ByVal value As String)
			Me.Item("hostName") = value
		End Set
	End Property

	<ConfigurationProperty("enableSsl", isrequired:=True, iskey:=False)> Public Property EnableSsl As Boolean
		Get
			Return Boolean.Parse(Me.Item("enableSsl").ToString)
		End Get
		Set(ByVal value As Boolean)
			Me.Item("enableSsl") = value
		End Set
	End Property

	<ConfigurationProperty("virtualPath", isrequired:=True, iskey:=False)> Public Property VirtualPath As String
		Get
			Return Me.Item("virtualPath").ToString
		End Get
		Set(ByVal value As String)
			Me.Item("virtualPath") = value
		End Set
	End Property

	<ConfigurationProperty("isMoved", isrequired:=False, iskey:=False)> Public Property IsMoved As Boolean
		Get
			Return Boolean.Parse(Me.Item("isMoved").ToString)
		End Get
		Set(ByVal value As Boolean)
			Me.Item("isMoved") = value
		End Set
	End Property

	<ConfigurationProperty("movedUrl", isrequired:=False, iskey:=False)> Public Property MovedUrl As String
		Get
			Return Me.Item("movedUrl").ToString
		End Get
		Set(ByVal value As String)
			Me.Item("movedUrl") = value
		End Set
	End Property

End Class