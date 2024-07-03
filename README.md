<h1 align="center" style="display: flex; align-items: center; justify-content: center; font-weight: 700;">
		<img src="https://raw.githubusercontent.com/Eshiunm/Rocket_HowOh/dev/src/assets/imgs/Howoh.ico" alt="HowOhLogo" />
	&nbsp;HowOh｜租屋交易與評價系統平台
</h1>
<div align="center" style="margin-bottom:24px">
  <a href="https://drive.google.com/file/d/1PA-nUPBaDxbWcsjRX8_U9CmcTLuKvKSq/view?usp=drive_link">
    簡報介紹
  </a>
  <span>｜</span>
  <a href="https://howoh.vercel.app/">
  佈署網址 
  </a>
  <span>｜</span>
  <a href="https://github.com/Eshiunm/Rocket_HowOh">
   前端 Github Repo 
  </a>
  <span>｜</span>
  <a href="https://github.com/Che1z/HowohBackEnd">
    後端 Github Repo 
  </a>
  <span>｜</span>
  <a href="http://52.140.100.60/swagger/ui/index">
  API swagger
  </a>
  <span>｜</span>
  <a href="https://smart-governor-e0d.notion.site/b5ca7a9893f9435ba967d608b0cbc2d4?pvs=4">
  Notion
  </a>
</div>

## 專案發想原由

根據內政部的數據推算，台灣的租屋人數總共為 255 萬人，約佔台灣總人口的 12%，相當於每 8 人就有 1 人是租屋者。<br>
此外，由於近年來房價所得比節節攀升，可以預期未來租屋人口的比例只會越來越高。<br>
目前租屋市場的主要痛點有**供需失衡**和**權力失衡**兩方面：<br>
由於房屋持有成本過低，市場上有許多房子完全閒置未使用，導致整體租屋市場呈現房東主導的局面。
這種供需失衡進一步導致了權力的不對等，租客的權利無法保障。<br>
此外，黑市問題也使得政府無法有效保障廣大租客的權益。<br>
最後，我們認為黑市問題導致的資訊不透明是可以改善的，因此有了『**好窩租屋交易與評價系統平台**』。

## 功能清單
### 租客身份功能
- 可建立個人帳號、大頭貼
- 可使用一般搜尋查找房源
- 可透過搜尋的條件快速過濾出房源
- 可透過房源列表進到單一房源頁面
- 可使用地圖搜尋查找房源
- 點選地圖標記導向單一房源頁面
- 可以針對喜歡的房源使用「預約看房」功能，發送預約看房資訊給房東
- 可在「租屋管理」頁面查看『預約看房』、『租約邀請』、『承租歷史』、『待評價』四個列表
  - 預約看房：查看自己預約了哪些房源
  - 租約邀請：查看自己收到哪些房東的租約邀請
  - 承租歷史：查看自己承租的歷史資訊
  - 待評價：租約到期後可以針對房源或房東進行評價
  
### 房東身份功能
- 可建立個人帳號、大頭貼
- 可使用一般搜尋查找自己所刊登的房源
- 可透過搜尋的條件快速過濾出房源
- 可透過房源列表進到單一房源頁面，檢視自己所刊登的房源資訊顯示狀態
- 可使用地圖搜尋查找自己的房源在地圖上的位置，以及鄰近周邊的設施與其他房源
- 點選地圖標記導向單一房源頁面
- 「房東好窩」頁面內含房東身分可使用的所有功能
  - 新增房源：房東可以依步驟刊登房源的資訊，並設定費用、租客限制等
    - 若尚未完成所有步驟，會歸類至『新增中』
  - 全部房源：依房東房源不同的狀態做分類
    - 指標區域：可查看各類別資料數量
      - 點擊『刊登中』、『已出租』、『已完成』：迅速跳到所屬分類區域
      - 點擊『待評價』：可開啟評價頁面（新分頁）
    - 新增中：尚未完成房源資訊的房源，可接續之前的部分繼續編輯
    - 刊登中：查看已成功刊登房源之內容
      - 更改房源為已出租：可指派租客（系統用戶、非系統用戶）承租
      - 更改房源為已完成：意即強制下架，不使用後續平台功能
      - 查看合約：可檢視該房源的合約資訊
      - 查看租客預約請求：可檢視該房源已預約之租客資訊列表，排序有舊至新、新至舊、已隱藏
    - 已出租：查看目前正在出租的房源資訊、租客資訊、租約起訖時間、建立合約
      - 建立合約：使用消基會定型化契約為範本，自動帶入房東、租客、房源資訊等
    - 已完成：可檢視房源內容，並可以在列表卡片上檢視可否評價
  - 出租歷史：房東可在此查看所有房源過往的出租歷史
<h3>後端 (Back-End)</h3>
 <p>

* 後端開發環境：
    * 框架：.NET Framework 4.7.2
    * 專案：ASP.NET Web API 2
* 後端開發技術：
  <div align="center">
    <img alt="Visual_Studio" src="https://img.shields.io/badge/Visual_Studio-5C2D91?style=for-the-badge&logo=visual%20studio&logoColor=white" />
    <img alt=".NET" src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
    <img alt="C#" src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" />
    <img alt="SQL" src="https://img.shields.io/badge/Microsoft%20SQL%20Server-CC2927?style=for-the-badge&logo=microsoft%20sql%20server&logoColor=white" />
    <img alt="Entity_Framework" src="https://img.shields.io/badge/Entity_Framework-yellow?style=for-the-badge">
    <img alt="LINQ" src="https://img.shields.io/badge/LINQ-8A2BE2?style=for-the-badge">
  </div>
  <div align="center">
    <img alt="Azure" src="https://img.shields.io/badge/microsoft%20azure-0089D6?style=for-the-badge&logo=microsoft-azure&logoColor=white" />
    <img alt="SignalR" src="https://img.shields.io/badge/SignalR-007ACC?style=for-the-badge&logoColor=white" />
    <img alt="GIT" src="https://img.shields.io/badge/GIT-E44C30?style=for-the-badge&logo=git&logoColor=white" />
    <img alt="GitHUB" src="https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white" />
    <img alt="POSTMAN" src="https://img.shields.io/badge/Postman-FF6C37?style=for-the-badge&logo=Postman&logoColor=white" />
    <img alt="OAuth2.0" src="https://img.shields.io/badge/OAuth_2.0-black?style=for-the-badge">
    <img alt="Passkey" src="https://img.shields.io/badge/Passkey-gray?style=for-the-badge&logo=webauthn">
    <img alt="Youtube" src="https://img.shields.io/badge/Youtube-FF0000?style=for-the-badge&logo=youtubestudio">
  </div>
  
  - 資料庫存取：Microsoft SQL Server 搭配 Entity Framework Code First 以及 LINQ 進行資料庫存取
  
  - 雲端服務：Azure 上建立虛擬機(VM)，並於 VM 上建立 SQL Server 與 IIS 環境，部署 Web API Application

  - 合約生成：以「消基會提供之定型化契約」為範本，透過 iTextSharp 套件進行合約建立並寫入契約相關個資，供使用者進行下載
 
  - 地圖搜尋：串接 Google Place API 進行搜尋地點的範圍計算，提供最佳結果供前端進行畫面渲染
  
 
