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
