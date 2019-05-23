# TODO: Make this nice
pipe.exe: pipe.cs
	mcs pipe.cs -debug -d:DEBUG -pkg:dotnet
