%.exe: *.cs
	mcs -out:$@ $^ -debug -d:DEBUG -pkg:dotnet

all: VisualPipes.exe

run: VisualPipes.exe
	MONO_TRACE_LISTENER=Console.Error mono --debug VisualPipes.exe

debug: VisualPipes.exe
	MONO_TRACE_LISTENER=Console.Error mono --verbose --debug VisualPipes.exe

clean:
	rm *.exe
	rm *.mdb
