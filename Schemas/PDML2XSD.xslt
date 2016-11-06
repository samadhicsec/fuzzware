<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:pdml="urn:Fuzzware.Schemer.Examples.PDML">

  <xsl:output method="xml" version="1.0" encoding="utf-8" indent="yes"/>

  <xsl:param name="prefix" select="'pdml'" />
  <xsl:param name="ns" select="'urn:Fuzzware.Examples.PDML'" />

  <xsl:template match="/">
    <xsl:element name="xs:schema" namespace="http://www.w3.org/2001/XMLSchema">
      <xsl:attribute name="elementFormDefault">qualified</xsl:attribute>
      <xsl:attribute name="targetNamespace">
        <xsl:value-of select="$ns"/>
      </xsl:attribute>
      <xsl:apply-templates select="pdml" />
    </xsl:element>
  </xsl:template>

  <xsl:template match="pdml">
    <xsl:element name="xs:element" namespace="http://www.w3.org/2001/XMLSchema">
      <xsl:attribute name="name">Packet</xsl:attribute>
      <xsl:attribute name="sac:markup" namespace="urn:Fuzzware.Schemas.SchemaAttributeCommands">removeIncludingChildNodes</xsl:attribute>
      <xsl:element name="xs:complexType" namespace="http://www.w3.org/2001/XMLSchema">
        <xsl:element name="xs:sequence" namespace="http://www.w3.org/2001/XMLSchema">
          <xsl:apply-templates select="packet" />
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="packet">
    <xsl:for-each select="proto">
      <xsl:if test="(@name != 'geninfo') 
              and (@name != 'frame') 
              and (@name != 'eth') 
              and (@name != 'ip') 
              and (@name != 'tcp')
              and (@name != 'udp')">
        <xsl:apply-templates select="." />
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="proto">
    <xsl:element name="xs:element" namespace="http://www.w3.org/2001/XMLSchema">
      <xsl:attribute name="name">
        <xsl:choose>
          <!-- Check if the 1st preceding sibling has the same name as the current -->
          <xsl:when test="preceding-sibling::proto[1]/@name = @name">
            <xsl:value-of select="@name" />
            <xsl:text>1</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@name" />
          </xsl:otherwise>
        </xsl:choose>
        <!--<xsl:value-of select="@name" />-->
      </xsl:attribute>
      <xsl:element name="xs:complexType" namespace="http://www.w3.org/2001/XMLSchema">
        <xsl:element name="xs:sequence" namespace="http://www.w3.org/2001/XMLSchema">
          <xsl:apply-templates select="field" />
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="field" >
    <!-- Make sure the name of the field is not empty -->
    <xsl:if test="@name != ''">
      <xsl:element name="xs:element" namespace="http://www.w3.org/2001/XMLSchema">
        <xsl:attribute name="name">
          <xsl:value-of select="@name" />
        </xsl:attribute>
        <!-- If the field has children, recurse -->
        <xsl:if test="count(child::*) > 0" >
          <!-- If the unmasked attribute is present on the children
          assume the children represents a bit list -->
          <xsl:if test="count(child::*/@unmaskedvalue) = 0" >
            <xsl:element name="xs:complexType" namespace="http://www.w3.org/2001/XMLSchema">
              <xsl:element name="xs:sequence" namespace="http://www.w3.org/2001/XMLSchema">
                <xsl:apply-templates select="field" />
              </xsl:element>
            </xsl:element>
          </xsl:if>
          <!-- Opposite condition, show the field value -->
          <xsl:if test="count(child::*/@unmaskedvalue) > 0" >
            <xsl:call-template name="simpletype_field" />
          </xsl:if>
        </xsl:if>
        <!-- No children, show the node value -->
        <xsl:if test="count(child::*) = 0" >
          <xsl:call-template name="simpletype_field" />
        </xsl:if>
      </xsl:element>
    </xsl:if>
    <!-- If the field name is empty try to recurse to any children fields -->
    <xsl:if test="@name = ''">
      <xsl:apply-templates select="field" />
    </xsl:if>
  </xsl:template>

  <xsl:template name="simpletype_field">
    <!-- If the element has a size then add as a restriction -->
    <xsl:if test="@size != ''">
      <xsl:call-template name="simpletype_field_with_restrictions" />
    </xsl:if>
    <xsl:if test="not(@size != '')">
      <xsl:call-template name="simpletype_field_without_restrictions" />
    </xsl:if>
  </xsl:template>

  <!-- Unrestricted field -->
  <xsl:template name="simpletype_field_without_restrictions">
    <xsl:attribute name="type">xs:hexBinary</xsl:attribute>
    <xsl:attribute name="sac:outputAs" namespace="urn:Fuzzware.Schemas.SchemaAttributeCommands">Decoded</xsl:attribute>
  </xsl:template>
  
  <!-- To fuzz effectively we need to restrict each value to the size value we get from the PDML-->
  <xsl:template name="simpletype_field_with_restrictions">
    <xsl:attribute name="sac:outputAs" namespace="urn:Fuzzware.Schemas.SchemaAttributeCommands">Decoded</xsl:attribute>
    <xsl:element name="xs:simpleType" namespace="http://www.w3.org/2001/XMLSchema">
      <xsl:element name="xs:restriction" namespace="http://www.w3.org/2001/XMLSchema">
        <xsl:attribute name="base">xs:hexBinary</xsl:attribute>
        <xsl:element name="xs:length" namespace="http://www.w3.org/2001/XMLSchema">
          <xsl:attribute name="value">
            <xsl:value-of select ="@size"/>
          </xsl:attribute>
        </xsl:element>           
      </xsl:element>
    </xsl:element>
  </xsl:template>
  
</xsl:stylesheet>
