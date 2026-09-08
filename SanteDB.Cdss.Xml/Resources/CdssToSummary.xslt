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
    ]]>
	</msxsl:script>
	<xsl:output method="html" indent="yes" omit-xml-declaration="yes" media-type="text/html"/>
	<xsl:template match="*" priority="-1"></xsl:template>

	<xsl:template match="/c:CdssLibraryCollection">
		<html>
			<head>
				<title>
					CDSS Library (Complete)
				</title>
				<style>
					<![CDATA[
					.tr-logic {
						background-color: #CCC;
					}
					.tr-fact {
						background-color: #CCF;
					}
					.tr-rule { 
						background-color: #CFC;
					}
					.tr-model {
						background-color: #FFC;
					}
					.tr-protocol {
						background-color: #CFC;
					}
					em.cd-keyword {
						margin-right: 1em;
						color: #00F;
						font-weight: bold
					}
					]]>
				</style>
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
	</xsl:template>
	<xsl:template match="/c:CdssLibrary">
		<html>
			<head>
				<title>
					<xsl:value-of select="@name"/>
				</title>
				<meta name="id" content="{@id}"/>
				<style>
					<![CDATA[
					.tr-logic {
						background-color: #CCC;
					}
					.tr-fact {
						background-color: #CCF;
					}
					.tr-rule { 
						background-color: #CFC;
					}
					.tr-model {
						background-color: #FFC;
					}
					.tr-protocol {
						background-color: #CFC;
					}
					em.cd-keyword {
						margin-right: 1em;
						color: #00F;
						font-weight: bold
					}
					]]>
				</style>
			</head>
			<body>
				<h1>
					<xsl:value-of select="@name"/>
					<small>
						(<xsl:value-of select="@id"/>)
					</small>
				</h1>
				<p>
					Version: <xsl:value-of select="c:meta/c:version"/> (state: <xsl:value-of select="c:status"/>
				</p>

				<h2>References</h2>
				<ul>
					<xsl:apply-templates select="c:include" />
				</ul>
				<h2>Definitions</h2>
				<table border="1">
					<tr>
						<th>Id</th>
						<th>Type</th>
						<th>Name</th>
						<th>Definition</th>
					</tr>
					<xsl:apply-templates select="c:logic" />
				</table>
			</body>
		</html>

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
			</td>
			<td>
				<xsl:if test="@name">
					<a name="{f:StripSpace(@name)}">
						<xsl:value-of select="@name"/>
					</a>
				</xsl:if>
			</td>
			<td>
				<strong>When:</strong>
				<xsl:apply-templates select="c:when" />
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
				<ul>
					<xsl:apply-templates select="c:all|c:any|c:none|c:csharp|c:hdsi|c:fact|c:query" mode="when" />
				</ul>
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
				<strong>When:</strong>
				<xsl:apply-templates select="c:when" />
				<strong>Then:</strong>
				<xsl:apply-templates select="c:then" />
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="c:model" mode="define">
		<tr>
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
				<pre>
					<xsl:value-of select="."/>
				</pre>
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
			<xsl:value-of select="."/>
		</code>
	</xsl:template>
	<xsl:template match="c:hdsi" mode="assign">
		<strong>HDSI:</strong>
		<code>
			<xsl:value-of select="."/>
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
			<xsl:value-of select="."/>
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
				<xsl:value-of select="@ref"/>
			</xsl:if>
			<xsl:if test="not(@ref)">

			</xsl:if>
		</li>
	</xsl:template>

	<xsl:template match="c:hdsi" mode="when">
		<li>
			<strong>HDSI:</strong>
			<code>
				<xsl:value-of select="."/>
			</code>

		</li>
	</xsl:template>

	<xsl:template match="c:csharp" mode="when">
		<li>
			<strong>C#:</strong>
			<code>
				<xsl:value-of select="."/>
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
			<xsl:value-of select="@select"/>
			<em class="cd-keyword"> From </em>
			<xsl:value-of select="@source"/>
			<em class="cd-keyword"> Where </em>
			<xsl:value-of select="."/>
			<xsl:if test="@order-by">
				<em class="cd-keyword"> Order by </em>
				<xsl:value-of select="@order-by"/>
			</xsl:if>
		</li>
	</xsl:template>


</xsl:stylesheet>