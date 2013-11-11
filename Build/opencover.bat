..\Tools\OpenCover\opencover.console.exe -register:user -target:runtests.bat -filter:"+[Newtonsoft.*]* -[*.Tests]*" -skipautoprops -hideskipped

..\Tools\ReportGenerator\ReportGenerator.exe -reports:results.xml -targetdir:.\reports