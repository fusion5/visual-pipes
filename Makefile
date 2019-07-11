%.exe: *.cs
	mcs -out:$@ $^ -debug -d:DEBUG -pkg:dotnet

all: VisualPipes.exe

run: VisualPipes.exe
	MONO_TRACE_LISTENER=Console.Error mono \
		--debug VisualPipes.exe "test1.pipes"

debug: VisualPipes.exe
	MONO_TRACE_LISTENER=Console.Error mono \
		--debug \
		VisualPipes.exe "test1.pipes"

debug_verbose: VisualPipes.exe
	MONO_TRACE_LISTENER=Console.Error mono \
		--verbose \
		--debug \
		VisualPipes.exe "test1.pipes"

clean:
	rm *.exe
	rm *.mdb
