%.exe: *.cs
	mcs -out:$@ $^ -debug -d:DEBUG -pkg:dotnet

all: VisualPipes.exe

run: VisualPipes.exe
	mono VisualPipes.exe

clean:
	rm *.exe
	rm *.mdb
