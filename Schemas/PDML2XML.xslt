<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    
  <xsl:output method="xml" version="1.0" encoding="utf-8" indent="yes"/>

  <xsl:param name="prefix" select="'pdml'" />
  <xsl:param name="ns" select="'urn:Fuzzware.Examples.PDML'" />
  
  <xsl:template match="/">
    <xsl:element name="{$prefix}:Packet" namespace="{$ns}">
      <xsl:apply-templates select="pdml" />
    </xsl:element>
  </xsl:template>

  <xsl:template match="pdml">
    <xsl:apply-templates select="packet" />
  </xsl:template>

  <xsl:template match="packet">    
    <xsl:for-each select="proto">
      <!-- Only select protocols from layer 4 up -->
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
    <!-- 2 proto's might have the same name, need to check, only a problem if they are consecutive -->
    <xsl:choose>
      <!-- Check if the 1st preceding sibling has the same name as the current -->
      <xsl:when test="preceding-sibling::proto[1]/@name = @name">
        <xsl:element name="{$prefix}:{@name}1" namespace="{$ns}">
          <xsl:apply-templates select="field" />
        </xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="{$prefix}:{@name}" namespace="{$ns}">
          <xsl:apply-templates select="field" />
        </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="field" >
    <!-- Make sure the name of the field is not empty -->
    <xsl:if test="@name != ''">
      <xsl:element name="{$prefix}:{@name}" namespace="{$ns}">
        <!-- If the field has children, recurse -->
        <xsl:if test="count(child::*) > 0" >
          <!-- If the unmasked attribute is present on the children
          assume the children represents a bit list -->
          <xsl:if test="count(child::*/@unmaskedvalue) = 0" >
            <xsl:apply-templates select="field" />
          </xsl:if>
          <!-- Opposite condition, show the field value -->
          <xsl:if test="count(child::*/@unmaskedvalue) > 0" >
            <xsl:value-of select="@value" />
          </xsl:if>
        </xsl:if>
        <!-- No children, show the node value -->
        <xsl:if test="count(child::*) = 0" >
          <xsl:value-of select="@value" />
        </xsl:if>  
      </xsl:element>
    </xsl:if>
    <!-- If the field name is empty try to recurse to any children fields -->
    <xsl:if test="@name = ''">
      <xsl:apply-templates select="field" />
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
