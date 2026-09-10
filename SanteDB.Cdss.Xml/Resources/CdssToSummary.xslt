<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl c i f"
                xmlns:c="http://santedb.org/cdss"
                xmlns="http://www.w3.org/1999/xhtml"
				xmlns:f="http://santedb.org/xsl-functions"
				xmlns:i="http://santedb.org/issue"

>

	<msxsl:script implements-prefix="f" language="C#">
		<![CDATA[

	public string StripSpace(string from) {
		return from.Replace(" ", "");
	}
	public string PrettyFix(string sourceCode) {
		sourceCode = sourceCode.Trim();
		if(sourceCode.Length < 80)
			return sourceCode;
		else {
			return sourceCode.Replace("&","& ").Replace("@"," @");
		}
	}
	public string CsvTable(string csvStr) {
		var inRows = csvStr.Trim().Split('\n');
		var retVal = new System.Text.StringBuilder();
		retVal.Append("<table border=\"1\">");
		var r = 0;
		foreach(var row in inRows) {
			var inCols = row.Split(',');
			var tag = r++ == 0 ? "th" : "td";
			retVal.Append("<tr>");
			foreach(var col in inCols) {
				retVal.AppendFormat("<{0}>{1}</{0}>", tag, col);
			}
			retVal.Append("</tr>");
		}
		retVal.Append("</table>");
		return retVal.ToString();
	}
	public string Unzip(String zipString) {
		using(var ms = new System.IO.MemoryStream(System.Convert.FromBase64String(zipString.Trim())))
        {
            using(var hr = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress))
            {
                using(var sr = new System.IO.StreamReader(hr))
                {
                    return sr.ReadToEnd();
                }
            }
        }
	}
    ]]>
	</msxsl:script>
	<xsl:output method="html" indent="yes" omit-xml-declaration="yes" media-type="text/html"/>
	<xsl:template match="*" priority="-1"></xsl:template>

	<xsl:template name="legend">
		<fieldset>
			<legend>CDSS Terminology / Legend</legend>
			<dl>
				<dt class="tr-fact">Fact</dt>
				<dd>An individual piece of data extracted from the CDR or computed</dd>
				<dt class="tr-model">Model</dt>
				<dd>A template used for the collection or expression of data in RIM</dd>
				<dt class="tr-rule">Rule</dt>
				<dd>
					A single condition which indicates <em>when</em> a condition is true, <em>then</em> one or more actions are taken
				</dd>
				<dt class="tr-protocol">Protocol</dt>
				<dd>A collection of rules which implement a single, continuious plan of care</dd>
			</dl>
		</fieldset>
	</xsl:template>
	<xsl:template name="styles">
		<style>
			<![CDATA[
					.tr-logic {
						background-color: #CCC;
					}
					.tr-fact {
						background-color: #CCF;
					}
					.tr-rule { 
						background-color: #FFC;
					}
					.tr-model {
						background-color: #CFF;
					}
					.tr-protocol {
						background-color: #CFC;
					}
					em.cd-keyword {
						margin-right: 1em;
						color: #00F;
						font-weight: bold
					}
					code {
						display: inline;
						overflow-wrap: anywhere;
					}
					dt {
						font-weight: bold;
						padding: 0.1em 0.25em;
					}
					
					]]>
		</style>
	</xsl:template>
	<xsl:template match="/c:CdssLibraryCollection">
		<html>
			<head>
				<title>
					CDSS Library (Complete)
				</title>
				<xsl:call-template name="styles" />
			</head>
			<body>
				<h1>CDSS Library Collection</h1>
				<table border="1">
					<tr>
						<th>Id</th>
						<th>Library</th>
						<th>Version</th>
						<th>Status</th>
					</tr>
					<xsl:apply-templates select="c:library" mode="summary" />
				</table>

				<xsl:call-template name="legend" />
				<hr/>
				
				<xsl:apply-templates select="c:library" mode="detail" />
			</body>
		</html>
	</xsl:template>
	<xsl:template match="c:library" mode="summary">
		<tr>
			<td>
				<a href="#{@id}">
					<xsl:value-of select="@id"/>
				</a>
			</td>
			<td>
				<a href="#{@id}">
					<xsl:value-of select="@name"/>
				</a>
			</td>
			<td>
				<xsl:value-of select="c:meta/c:version"/>
			</td>
			<td>
				<xsl:value-of select="c:status"/>
			</td>
		</tr>
	</xsl:template>
	<xsl:template match="c:library" mode="detail">
		<a name="{@id}">
			<h2>
				<xsl:value-of select="@name"/>
				<small>
					(<xsl:value-of select="@id"/>)
				</small>
			</h2>
		</a>
		<p>
			Version: <xsl:value-of select="c:meta/c:version"/> (state: <xsl:value-of select="c:status"/>)
		</p>
		<xsl:if test="c:meta/c:documentation">
			<p>
				<xsl:value-of select="c:meta/c:documentation"/>
			</p>
		</xsl:if>
		<xsl:if test="c:include">
			<h3>References</h3>
			<ul>
				<xsl:apply-templates select="c:include" />
			</ul>
		</xsl:if>
		<xsl:if test="c:logic">
			<h3>Definitions</h3>
			<table border="1">
				<tr>
					<th>Id</th>
					<th>Type</th>
					<th>Name</th>
					<th>Definition</th>
				</tr>
				<xsl:apply-templates select="c:logic" />
			</table>
		</xsl:if>
		<xsl:if test="c:data">
			<h3>Data</h3>
			<xsl:apply-templates select="c:data" />
		</xsl:if>
	</xsl:template>
	<xsl:template match="/c:CdssLibrary">
		<html>
			<head>
				<title>
					<xsl:value-of select="@name"/>
				</title>
				<meta name="id" content="{@id}"/>
				<xsl:call-template name="styles" />
			</head>
			<body>
				<h1>
					<xsl:value-of select="@name"/>
					<small>
						(<xsl:value-of select="@id"/>)
					</small>
				</h1>
				<p>
					Version: <xsl:value-of select="c:meta/c:version"/> (state: <xsl:value-of select="c:status"/>)
				</p>
				<xsl:if test="c:meta/c:documentation">
					<p>
						<xsl:value-of select="c:meta/c:documentation"/>
					</p>
				</xsl:if>
				<xsl:if test="c:include">
					<h3>References</h3>
					<ul>
						<xsl:apply-templates select="c:include" />
					</ul>
				</xsl:if>
				<xsl:if test="c:logic">
					<h3>Definitions</h3>
					<table border="1">
						<tr>
							<th>Id</th>
							<th>Type</th>
							<th>Name</th>
							<th>Definition</th>
						</tr>
						<xsl:apply-templates select="c:logic" />
					</table>
				</xsl:if>
				<xsl:if test="c:data">
					<h3>Data</h3>
					<xsl:apply-templates select="c:data" />
				</xsl:if>

				<xsl:call-template name="legend" />

			</body>
		</html>

	</xsl:template>

	<xsl:template match="c:data">
		<h4>
			<xsl:value-of select="@name"/>
		</h4>
		<xsl:if test="c:meta/c:documentation">
			<p>
				<xsl:value-of select="c:meta/c:documentation"/>
			</p>
		</xsl:if>
		<xsl:value-of disable-output-escaping="yes" select="f:CsvTable(f:Unzip(text()[last()]))"/>
	</xsl:template>
	<xsl:template match="c:logic">
		<tr class="tr-logic">
			<td>
				<xsl:if test="@id">
					<a name="{f:StripSpace(@id)}">
						<xsl:value-of select="@id"/>
					</a>
				</xsl:if>
			</td>
			<td>
				Logic Block
				<xsl:if test="c:context/@type">
					<em>
						(<xsl:value-of select="c:context/@type"/>)
					</em>
				</xsl:if>
			</td>
			<td>
				<xsl:if test="@name">
					<a name="{f:StripSpace(@name)}">
						<xsl:value-of select="@name"/>
					</a>
				</xsl:if>
			</td>
			<td>
				<xsl:if test="c:meta/c:documentation">
					<p>
						<xsl:value-of select="c:meta/c:documentation/text()"/>
					</p>
				</xsl:if>

				<xsl:if test="c:when">
					<strong>When:</strong>
					<xsl:apply-templates select="c:when" />
				</xsl:if>
			</td>
		</tr>
		<xsl:apply-templates select="c:define/*" mode="define"/>
	</xsl:template>

	<xsl:template match="c:fact" mode="define">
		<tr class="tr-fact">
			<td>
				<xsl:if test="@id">
					<a name="{f:StripSpace(@id)}">
						<xsl:value-of select="@id"/>
					</a>
				</xsl:if>
			</td>
			<td>Fact</td>
			<td>
				<xsl:if test="@name">
					<a name="{f:StripSpace(@name)}">
						<xsl:value-of select="@name"/>
					</a>
				</xsl:if>
			</td>
			<td>
				<xsl:if test="c:meta/c:documentation">
					<p>
						<xsl:value-of select="c:meta/c:documentation/text()"/>
					</p>
				</xsl:if>
				<ul>
					<xsl:apply-templates select="c:all|c:any|c:none|c:csharp|c:hdsi|c:fact|c:query" mode="when" />
				</ul>

				<xsl:if test="c:normalize">
					<table>
						<caption>Normalize Value</caption>
						<tr>
							<th>When</th>
							<th>Computed By</th>
						</tr>
						<xsl:apply-templates select="c:normalize" />
					</table>
				</xsl:if>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="c:normalize">
		<tr>
			<td>
				<xsl:apply-templates select="c:when/c:csharp|c:when/c:fact|c:when/c:hdsi" mode="assign"/>
			</td>
			<td>
				<xsl:apply-templates select="c:csharp|c:fact|c:hdsi" mode="assign"/>
			</td>
		</tr>
	</xsl:template>
	<xsl:template match="c:rule" mode="define">
		<tr class="tr-rule">
			<td>
				<xsl:if test="@id">
					<a name="{f:StripSpace(@id)}">
						<xsl:value-of select="@id"/>
					</a>
				</xsl:if>
			</td>
			<td>Rule</td>
			<td>
				<xsl:if test="@name">
					<a name="{f:StripSpace(@name)}">
						<xsl:value-of select="@name"/>
					</a>
				</xsl:if>
			</td>
			<td>
				<xsl:if test="c:meta/c:documentation">
					<p>
						<xsl:value-of select="c:meta/c:documentation/text()"/>
					</p>
				</xsl:if>
				<strong>When:</strong>
				<xsl:apply-templates select="c:when" />
				<strong>Then:</strong>
				<xsl:apply-templates select="c:then" />
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="c:protocol" mode="define">
		<tr class="tr-protocol">
			<td>
				<xsl:if test="@id">
					<a name="{f:StripSpace(@id)}">
						<xsl:value-of select="@id"/>
					</a>
				</xsl:if>
			</td>
			<td>Protocol</td>
			<td>
				<xsl:if test="@name">
					<a name="{f:StripSpace(@name)}">
						<xsl:value-of select="@name"/>
					</a>
				</xsl:if>
			</td>
			<td>
				<xsl:if test="c:meta/c:documentation">
					<p>
						<xsl:value-of select="c:meta/c:documentation/text()"/>
					</p>
				</xsl:if>
				<xsl:if test="c:scopes/c:add">
					<p>
						<strong>Applicable Care Pathways/Visit Types</strong>
					</p>
					<ul>
						<xsl:apply-templates select="c:scopes/*" mode="visit" />
					</ul>
				</xsl:if>
				<strong>When:</strong>
				<xsl:apply-templates select="c:when" />
				<strong>Then:</strong>
				<xsl:apply-templates select="c:then" />
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="c:add" mode="visit">
		<li>
			<xsl:choose>
				<xsl:when test="@id = 'org.santedb.emr.patient.registration'">Patient Registration</xsl:when>
				<xsl:when test="@id = 'org.santedb.emr.visit.anc'">ANC Visit</xsl:when>
				<xsl:when test="@id = 'org.santedb.emr.act.visit.general'">General/Primary Care</xsl:when>
				<xsl:when test="@id = 'org.santedb.ims.pediatric.routineVacc'">Routine Vaccination</xsl:when>
				<xsl:when test="@id = 'org.santedb.emr.act.registration.birth'">Birth Registration</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@id"/>
				</xsl:otherwise>
			</xsl:choose>
		</li>
	</xsl:template>
	<xsl:template match="c:model" mode="define">
		<tr class="tr-model">
			<td>
				<xsl:if test="@id">
					<a name="{f:StripSpace(@id)}">
						<xsl:value-of select="@id"/>
					</a>
				</xsl:if>
			</td>
			<td>Model</td>
			<td>
				<xsl:if test="@name">
					<a name="{f:StripSpace(@name)}">
						<xsl:value-of select="@name"/>
					</a>
				</xsl:if>
			</td>
			<td>
				<xsl:if test="c:meta/c:documentation">
					<p>
						<xsl:value-of select="c:meta/c:documentation/text()"/>
					</p>
				</xsl:if>
				<xsl:if test="c:json">
					<pre>
						<xsl:value-of select="c:json/text()"/>
					</pre>
				</xsl:if>
				<xsl:if test="@extern">
					<strong>External Template: </strong>
					<xsl:value-of select="@extern"/>
				</xsl:if>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="c:when">
		<ul>
			<xsl:apply-templates select="c:all|c:any|c:none|c:csharp|c:hdsi|c:fact" mode="when" />
		</ul>
	</xsl:template>

	<xsl:template match="c:then">
		<ol>
			<xsl:apply-templates select="c:propose|c:assign|c:raise|c:repeat|c:apply|c:rule" mode="then" />
		</ol>
	</xsl:template>


	<xsl:template match="c:assign" mode="then">
		<li>
			<strong>Assign </strong>
			<xsl:value-of select="@path"/>
			=
			<xsl:apply-templates select="c:csharp|c:hdsi|c:query|c:fact|c:fixed" mode="assign" />
		</li>
	</xsl:template>
	<xsl:template match="c:apply" mode="then">
		<li>
			<a href="#{f:StripSpace(@ref)}">
				<xsl:value-of select="@ref"/>
			</a>
		</li>
	</xsl:template>
	<xsl:template match="c:raise" mode="then">
		<li>
			<strong>
				RAISE <xsl:value-of select="i:issue/@priority"/>:
			</strong>
			<xsl:value-of select="i:issue/text()"/>
		</li>
	</xsl:template>
	<xsl:template match="c:repeat" mode="then">
		<li>
			<strong>
				RELEAT
				<xsl:if test="@iterations">
					<em class="cd-keyword">
						FOR <xsl:value-of select="@iterations"/> TIMES
					</em>
				</xsl:if>
				<xsl:if test="c:until">
					<em class="cd-keyword">
						UNTIL
						<ul>
							<xsl:apply-templates select="c:until/*" mode="when" />
						</ul>
					</em>
				</xsl:if>
				<xsl:if test="@trackBy">
					TRACK BY <xsl:value-of select="@trackBy"/>
				</xsl:if>
			</strong>
			<xsl:apply-templates select="c:execute" />
		</li>
	</xsl:template>
	<xsl:template match="c:execute">
		<ol>
			<xsl:apply-templates select="c:propose|c:assign|c:raise|c:repeat|c:apply|c:rule" mode="then" />
		</ol>
	</xsl:template>
	<xsl:template match="c:propose" mode="then">
		<li>
			<em>
				Propose: <xsl:value-of select="@name"/>
			</em>

			<xsl:if test="c:model/c:json">
				<pre>
					<xsl:value-of select="c:model/c:json"/>
				</pre>
			</xsl:if>
			<xsl:if test="c:model/@ref">
				<a href="#{f:StripSpace(c:model/@ref)}">
					<xsl:value-of select="c:model/@ref"/>
				</a>
			</xsl:if>
			<xsl:if test="c:model/@extern">
				<xsl:value-of select="c:model/@extern"/>
			</xsl:if>

			<xsl:if test="c:assign">
				<em>Assign</em>
				<table>
					<xsl:apply-templates select="c:assign" mode="assign" />
				</table>
			</xsl:if>
		</li>
	</xsl:template>

	<xsl:template match="c:assign" mode="assign">
		<tr>
			<td>
				<xsl:value-of select="@path"/>
			</td>
			<td>=</td>
			<td>
				<xsl:apply-templates select="c:csharp|c:hdsi|c:query|c:fact|c:fixed" mode="assign" />
			</td>
		</tr>
	</xsl:template>
	<xsl:template match="c:csharp" mode="assign">
		<strong>C#:</strong>
		<code>
			<xsl:value-of select="f:PrettyFix(.)"/>
		</code>
	</xsl:template>
	<xsl:template match="c:hdsi" mode="assign">
		<strong>HDSI:</strong>
		<code>
			<xsl:value-of select="f:PrettyFix(.)"/>
		</code>
	</xsl:template>
	<xsl:template match="c:fact" mode="assign">
		<a href="#{f:StripSpace(@ref)}">
			<xsl:value-of select="@ref"/>
		</a>
	</xsl:template>
	<xsl:template match="c:fixed" mode="assign">
		<strong>CONST:</strong>
		<code>
			<xsl:value-of select="f:PrettyFix(.)"/>
		</code>
	</xsl:template>

	<xsl:template match="c:all" mode="when">
		<li>
			<strong>All Of</strong>
			<ul>
				<xsl:apply-templates select="c:all|c:any|c:none|c:csharp|c:hdsi|c:fact" mode="when" />
			</ul>
		</li>
	</xsl:template>

	<xsl:template match="c:any" mode="when">
		<li>
			<strong>Any Of</strong>
			<ul>
				<xsl:apply-templates select="c:all|c:any|c:none|c:csharp|c:hdsi|c:fact" mode="when" />
			</ul>
		</li>
	</xsl:template>

	<xsl:template match="c:none" mode="when">
		<li>
			<strong>None Of</strong>
			<ul>
				<xsl:apply-templates select="*" mode="when" />
			</ul>
		</li>
	</xsl:template>

	<xsl:template match="c:fact" mode="when">
		<li>
			<xsl:if test="@ref">
				<a href="#{f:StripSpace(@ref)}">
					<xsl:value-of select="@ref"/>
				</a>
			</xsl:if>
			<xsl:if test="not(@ref)">
				TODO
			</xsl:if>
		</li>
	</xsl:template>

	<xsl:template match="c:hdsi" mode="when">
		<li>
			<strong>HDSI:</strong>
			<code>
				<xsl:value-of select="f:PrettyFix(.)"/>
			</code>

		</li>
	</xsl:template>

	<xsl:template match="c:csharp" mode="when">
		<li>
			<strong>C#:</strong>
			<code>
				<xsl:value-of select="f:PrettyFix(.)"/>
			</code>
		</li>
	</xsl:template>
	<xsl:template match="c:include">
		<li>
			<xsl:value-of select="text()"/>
		</li>
	</xsl:template>

	<xsl:template match="c:query" mode="when">
		<li>
			<strong>CDR Query</strong>
			<em class="cd-keyword">
				Select <xsl:value-of select="@fn"/>
			</em>
			<code>
				<xsl:value-of select="f:PrettyFix(@select)"/>
			</code>
			<em class="cd-keyword"> From </em>
			<code>
				<xsl:value-of select="f:PrettyFix(@source)"/>
			</code>
			<em class="cd-keyword"> Where </em>
			<code>
				<xsl:value-of select="f:PrettyFix(.)"/>
			</code>
			<xsl:if test="@order-by">
				<em class="cd-keyword"> Order by </em>
				<code>
					<xsl:value-of select="@order-by"/>
				</code>
			</xsl:if>
		</li>
	</xsl:template>


</xsl:stylesheet>