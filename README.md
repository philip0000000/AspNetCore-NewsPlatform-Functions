# AFHP_NewsSite_Functions  
_Serverless background tasks for the AFHP News Site_

_Last updated: 2025-12-12_

**This repository is a sanitized personal copy maintained for portfolio purposes.**
(See https://github.com/philip0000000/AspNetCore-NewsPlatform for the project this project is assosiated with)

---

## Overview  
This project contains the Azure Functions used by the **AFHP News Site**. <br/>
The functions run independently from the main web application and handle automated tasks such as newsletters, archiving, image resizing, and API data collection.

---

## Functions Included

- **Image Resize (Blob Trigger)**  
  Creates resized thumbnail images when an article image is uploaded.

- **Newsletter Sender (Timer Trigger)**  
  Sends scheduled newsletters to subscribed users.

- **Archive Old Articles (Timer Trigger)**  
  Automatically archives news older than a set number of days.

- **External API Data Save (Timer Trigger)**  
  Fetches and stores historical data (weather, currency, electricity, business).

- **HTTP Test Function**  
  Simple endpoint used for testing the Function App.

---

## Running the Functions  
The project can be run locally using **Azure Functions Core Tools** or **Visual Studio**.  
Each Function App folder contains the code and settings needed to run its triggers.

---

## Deployment  
When deployed to Azure, each function runs automatically based on its trigger (timer, blob upload, or HTTP).

---

## Contributors  
- hanlilj  
- PollyPinkPro  
- azadeh-k1  
- philip0000000  

## License
This project is created for educational purposes as part of the **Lexicon .NET Training Program (2025)**.  
Not intended for commercial use.

