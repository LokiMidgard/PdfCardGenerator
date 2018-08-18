# PdfCardGenerator

This project aims to create playing cards based on an two input xml documents.
First a data xml that contains information about the cards and a template xml that transforms those data in the actual pdf.

The Template uses xpath to select elements. The following sample would create for every Action element under Actions Data of the data xml
one PDF page and add the text that is set in the Name attribute of the Action element to the page. 

```xml
<Project xmlns="https://raw.githubusercontent.com/LokiMidgard/PdfCardGenerator/master/PdfCardGenerator/XMLImport.xsd"
  DefaultLanguage="de-DE">
  <Fonts UseSystemFallback="true" DefaultFont="Calibri">
  </Fonts>
  <FallbackFonts>
  </FallbackFonts>
  <Templates>
    <Template Context="//Data/Actions/Action" Width="64.9 mm" Height="97 mm">
    <!-- Title-->
      <Text left="9.5 mm" top="5 mm" width="50 mm" height="5.76 mm"  VerticalAlignment="Center">
        <Paragraph  Alignment="Center" EmSize="12" FontStyle="Bold">
          <TextRun>
            <TextPath>@Name</TextPath>
          </TextRun>
        </Paragraph>
      </Text>
    </Template>
  </Templates>
</Project>
```
