log4net.confi

<log4net>
  <root>
    <level value="ALL" />
    <appender-ref ref="console" />
    <appender-ref ref="UdpAppender" />
  </root>
  <appender name="console" type="log4net.Appender.ConsoleAppender">
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date %level %logger - %message%newline" />
    </layout>
  </appender>
  <appender name="UdpAppender" type="log4net.Appender.UdpAppender">
    <localPort value="4447" />
    <remoteAddress value="127.0.0.1" />
    <remotePort value="7071" />
    <layout type="log4net.Layout.XmlLayout" />
  </appender>
</log4net>



nlog.config
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.mono2.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogLevel="Warn"
      internalLogFile="nlog log.log"
      >
  <variable name="VerboseLayout"
            value="${longdate} ${level:upperCase=true} ${message}  
                    (${callsite:includSourcePath=true})"            />
  <variable name="ExceptionVerboseLayout"
            value="${VerboseLayout} (${stacktrace:topFrames=10})  
                     ${exception:format=ToString}"                  />

  <targets async="true">

    <target name="colouredConsole" xsi:type="ColoredConsole" useDefaultRowHighlightingRules="false"
       layout="${longdate}|${pad:padding=5:inner=${level:uppercase=true}}|${message}" >
      <highlight-row condition="level == LogLevel.Debug" foregroundColor="DarkGray" />
      <highlight-row condition="level == LogLevel.Info" foregroundColor="Gray" />
      <highlight-row condition="level == LogLevel.Warn" foregroundColor="Yellow" />
      <highlight-row condition="level == LogLevel.Error" foregroundColor="Red" />
      <highlight-row condition="level == LogLevel.Fatal" foregroundColor="Red" backgroundColor="White" />
    </target>

    <target name="log4view" xsi:type="NLogViewer" address="tcp://127.0.0.1:4447"/>
    
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="log4view" />
  </rules>

</nlog>

