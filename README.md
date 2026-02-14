# PictureBox

![NET](https://img.shields.io/badge/NET-8.0-green.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)
![VS2022](https://img.shields.io/badge/Visual%20Studio-2022-white.svg)
![Version](https://img.shields.io/badge/Version-1.0.2025.0-yellow.svg)]

Das Demo zeigt ein UserControl **PictureBox** das eine Liste von Bilder darstellen kann. Zusätzlich gibt es eine Navigationsleiste für Funktionen wie *Auf- und Abblättern, Löschen, Hinzufügen usw.*

<img src="ControlMain.png" style="width:650px;"/>

Desweiteren zeigt das kleine Projekt wie ein Base64 String zur Laufzeit in der PictureBox dargestellt werden kann.

Die Umwandlung eines Base64 kodierter String läßt sich denkbar einfach in ein BitmapSource umwandel.und direkt im **WPF Image-Control** dargestellt werden.
```csharp
private static BitmapImage Base64ToImageSource(string base64String)
{
    BitmapImage bi = new BitmapImage();

    bi.BeginInit();
    bi.StreamSource = new MemoryStream(System.Convert.FromBase64String(base64String));
    bi.EndInit();

    return bi;
}
```

