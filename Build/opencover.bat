..\Tools\OpenCover\opencover.console.exe -register:user -target:runtests.bat -filter:"+[Newtonsoft.*]* -[*.Tests]*"

..\Tools\ReportGenerator\ReportGenerator.exe -reports:results.xml -targetdir:.\reports