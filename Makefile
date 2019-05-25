%.exe: %.cs
	mcs $< -debug -d:DEBUG -pkg:dotnet

all: VisualPipes.exe
