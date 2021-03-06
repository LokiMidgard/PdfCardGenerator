<xsl:stylesheet
        xmlns:nota="http://nota.org/schema/nota"
        xmlns:lebewesen="http://nota.org/schema/lebewesen"
        xmlns:kultur="http://nota.org/schema/kultur"
        xmlns:profession="http://nota.org/schema/profession"
        xmlns:talent="http://nota.org/schema/talent"
        xmlns:aktionen="http://nota.org/schema/kampf/aktionen"
        xmlns:fertigkeit="http://nota.org/schema/fertigkeit"
        xmlns:besonderheit="http://nota.org/schema/besonderheit"
                xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
 xsi:schemaLocation="http://nota.org/schema/nota ..\..\nota\input\static\schema\nota.xsd
                http://nota.org/schema/lebewesen ..\..\nota\input\static\schema\lebewesen.xsd
                http://nota.org/schema/kampf/aktionen ..\..\nota\input\static\schema\kampf\aktionen.xsd
                http://nota.org/schema/kultur ..\..\nota\input\static\schema\kultur.xsd
                http://nota.org/schema/profession ..\..\nota\input\static\schema\profession.xsd
                http://nota.org/schema/talent ..\..\nota\input\static\schema\talent.xsd
                http://nota.org/schema/fertigkeit ..\..\nota\input\static\schema\fertigkeit.xsd 
                http://nota.org/schema/besonderheit ..\..\nota\input\static\schema\besonderheit.xsd"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template match="nota:Daten">
    <xsl:apply-templates select="aktionen:Aktionen"/>
  </xsl:template>

  <xsl:template match="aktionen:Aktionen">
    <Aktionen>
      <xsl:apply-templates select="aktionen:Aktion"/>
    </Aktionen>
  </xsl:template>

  <xsl:template match="aktionen:Aktion">
    <Aktion>
      <Name>
        <xsl:value-of select="@Name"/>&#xfeff;
      </Name>
      <Kosten>
        <xsl:value-of select="@Kosten"/>&#xfeff;
      </Kosten>
      <Typ>
        <xsl:choose>
          <xsl:when test="@Typ='Offensiv'">
            Offensiv
          </xsl:when>
          <xsl:when test="@Typ='Defensiv'">
            Defensiv
          </xsl:when>
          <xsl:when test="@Typ='Ausgeglieschen'">
            Offensiv und Passiv
          </xsl:when>
          <xsl:when test="@Typ='Frei'">
            Frei
          </xsl:when>
          <xsl:when test="@Typ='Sekundär'">
            Sekundär
          </xsl:when>
          <xsl:when test="@Typ='Unterstützend'">
            Unterstützend
          </xsl:when>
          <xsl:otherwise>
            [Ungültiger wert] (<xsl:value-of select="@Typ"/>)
          </xsl:otherwise>
        </xsl:choose>
      </Typ>

      <xsl:choose>
        <xsl:when test="./aktionen:Mod/@ModifierType='Bonus'">
          <Modifikation>
            + <xsl:apply-templates select="aktionen:Mod/*"/>&#xfeff;
          </Modifikation>
        </xsl:when>
        <xsl:when test="./aktionen:Mod/@ModifierType='Malus'">
          <Modifikation>
            &#x2212;<xsl:apply-templates select="aktionen:Mod/*"/>&#xfeff;
          </Modifikation>
        </xsl:when>
        <xsl:otherwise>
          <Modifikation>
            -
          </Modifikation>
        </xsl:otherwise>
      </xsl:choose>
      <Beschreibung>
        <xsl:value-of select="aktionen:Beschreibung"/>&#xfeff;
      </Beschreibung>
      <xsl:if test="./aktionen:Bedingung">
        <Titel>
          Bedingung
        </Titel>
        <Text>
          <xsl:value-of select="./aktionen:Bedingung"/>&#xfeff;
        </Text>
      </xsl:if>
      <xsl:if test="./aktionen:InstantEffekt">
        <Titel>
          Augenblicklicher Effekt
        </Titel>
        <Text>
          <xsl:value-of select="./aktionen:InstantEffekt"/>&#xfeff;
        </Text>
      </xsl:if>
      <xsl:if test="./aktionen:GarantierterEffekt">
        <Titel>
          Garantierter Effekt
        </Titel>
        <Text>
          <xsl:value-of select="./aktionen:GarantierterEffekt"/>&#xfeff;
        </Text>
      </xsl:if>
      <xsl:if test="./aktionen:OffensivErfolg">
        <Titel>
          Offensiver Erfolg
        </Titel>
        <Text>
          <xsl:value-of select="./aktionen:OffensivErfolg"/>&#xfeff;
        </Text>
      </xsl:if>
      <xsl:if test="./aktionen:DefensivErfolg">
        <Titel>
          Defensiver Erfolg
        </Titel>
        <Text>
          <xsl:value-of select="./aktionen:DefensivErfolg"/>&#xfeff;
        </Text>
      </xsl:if>
      <xsl:if test="./aktionen:SekundärErfolg">
        <Titel>
          Säkunderer Erfolg
        </Titel>
        <Text>
          <xsl:value-of select="./aktionen:SekundärErfolg"/>&#xfeff;
        </Text>
      </xsl:if>
      <xsl:if test="./aktionen:OffensivMiserfolg">
        <Titel>
          Offensiver Misserfolg
        </Titel>
        <Text>
          <xsl:value-of select="./aktionen:OffensivMiserfolg"/>&#xfeff;
        </Text>
      </xsl:if>
      <xsl:if test="./aktionen:DefensivMiserfolg">
        <Titel>
          Defensiver Misserfolg
        </Titel>
        <Text>
          <xsl:value-of select="./aktionen:DefensivMiserfolg"/>&#xfeff;
        </Text>
      </xsl:if>
      <xsl:if test="./aktionen:OffensivMiserfolg">
        <Titel>
          Offensiver Misserfolg
        </Titel>
        <Text>
          <xsl:value-of select="./aktionen:OffensivMiserfolg"/>&#xfeff;
        </Text>
      </xsl:if>
      <xsl:if test="./aktionen:DefensivMiserfolg">
        <Titel>
          Defensiver Misserfolg
        </Titel>
        <Text>
          <xsl:value-of select="./aktionen:DefensivMiserfolg"/>&#xfeff;
        </Text>
      </xsl:if>
      <xsl:if test="./aktionen:SekundärMiserfolg">
        <Titel>
          Säkunderer Misserfolg
        </Titel>
        <Text>
          <xsl:value-of select="./aktionen:SekundärMiserfolg"/>&#xfeff;
        </Text>
      </xsl:if>


    </Aktion>
  </xsl:template>


  <xsl:template match="aktionen:Mod">ModificaitonSample</xsl:template>

  <xsl:template match="*">
    <Error>
      [FEHLER IM XSLT] <xsl:value-of select ="name(.)"/>
    </Error>
  </xsl:template>

  <xsl:template match="aktionen:ConcreteModValueType">
    <xsl:value-of select="@Value"/>
    <xsl:if test="@Type='Percent'">%</xsl:if>
  </xsl:template>

  <xsl:template match="aktionen:VariableModValueType">
    <xsl:value-of select="@Value"/>
  </xsl:template>

  <xsl:template match="aktionen:AddModValueType">
    <xsl:apply-templates select="./*[1]"/>
    <xsl:for-each select="./*[position()>1]">
      + <xsl:apply-templates select="."/>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="aktionen:SubstractModValueType">
    <xsl:apply-templates select="./*[1]"/>
    <xsl:for-each select="./*[position()>1]">
      - <xsl:apply-templates select="."/>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="aktionen:MultiplyModValueType">
    <xsl:apply-templates select="./*[1]"/>
    <xsl:for-each select="./*[position()>1]">
      &#8226; <xsl:apply-templates select="."/>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="*">
    [[FEHLER IM XSLT]] (<xsl:value-of select="local-name()"/>)
  </xsl:template>


</xsl:stylesheet>