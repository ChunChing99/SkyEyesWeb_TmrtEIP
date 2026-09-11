# OpenLayers 圖台整合指南

> ⚠️ **待確認是否適用**：本文件沿用自更早的模版專案（內文甚至誤留 `skyeyesweb_bhcmu.client` 路徑，非本專案），描述以 OpenLayers 整合地圖。中捷 EIP 是否需要地圖/圖台功能尚未確認；若不需要地圖功能可忽略/移除本文件，若需要則應重新確認整合方式後改寫。

## 架構說明
- **地圖模組**: `skyeyesweb_bhcmu.client\src\ol.ts` - OpenLayers 核心封裝與 MarkerManager
- **整合方式**: 直接在 Vue 組件中建立 OpenLayers Map 實例
- **標記管理器**: 使用自訂的 `MarkerManager` 類別管理地圖標記、折線、多邊形等圖層

## 核心模組 (ol.ts)

### 匯出項目
```typescript
// OpenLayers 核心
export { Map, View, Overlay }
export type { MapBrowserEvent }

// 圖層
export { TileLayer, VectorLayer }

// 資料源
export { XYZ, VectorSource, Cluster }

// 要素與幾何
export { Feature, Point, LineString, Polygon }
export { fromLonLat, toLonLat }

// 樣式
export { Style, Icon, Text, Fill, Stroke, CircleStyle }

// 渲染工具
export { getVectorContext }
export type { RenderEvent }

// 標記管理器
export { MarkerManager }
```

## Vue 組件整合範例

### 1. 模板結構
```vue
<template>
  <Master01>
    <!-- 地圖容器 -->
    <div id="map" class="map-container"></div>

    <!-- 右鍵選單 -->
    <ContextMenu ref="mapContextMenu" :model="contextMenuItems" />
  </Master01>
</template>
```

### 2. TypeScript 控制邏輯
```typescript
import { defineComponent } from 'vue';
import { Map, View, TileLayer, XYZ, fromLonLat, MarkerManager } from '@/ol';
import type { AddMarkerOptions } from '@/ol';

export default defineComponent({
  data() {
    return {
      map: null as Map | null,
      manager: null as MarkerManager | null,
      // 圖磚來源設定
      tileSources: [
        {
          name: 'EasyMap',
          url: 'https://map.skyeyes.tw/EasyMap6/{z}/{x}/z{z}x{x}y{y}.png',
        },
        {
          name: 'OpenStreetMap',
          url: 'https://{a-c}.tile.openstreetmap.org/{z}/{x}/{y}.png',
        },
      ],
      selectedTileSource: 'EasyMap',
    };
  },
  mounted() {
    this.initMap();
  },
  beforeUnmount() {
    this.manager?.dispose();
  },
  methods: {
    // 初始化地圖
    initMap() {
      const tileSource = this.tileSources.find(s => s.name === this.selectedTileSource);

      this.map = new Map({
        target: 'map',
        layers: [
          new TileLayer({
            source: new XYZ({
              url: tileSource?.url,
            }),
          }),
        ],
        view: new View({
          center: fromLonLat([121.0, 23.5]),  // 台灣中心
          zoom: 8,
          minZoom: 8,
          maxZoom: 18,
        }),
      });

      // 建立標記管理器
      this.manager = new MarkerManager(this.map);

      // 設定右鍵選單
      this.setupContextMenu();
    },

    // 設定右鍵選單
    setupContextMenu() {
      this.map!.getViewport().addEventListener('contextmenu', (evt) => {
        evt.preventDefault();
        const pixel = this.map!.getEventPixel(evt);
        const coordinate = this.map!.getCoordinateAt(pixel);
        const [lon, lat] = toLonLat(coordinate);

        this.clickedCoordinate = [lon, lat];
        this.$refs.mapContextMenu.show(evt);
      });
    },
  }
});
```

## MarkerManager 使用指南

### 1. 加入標記（Marker）

#### 基本標記
```typescript
// 建立圖示元素
const img = document.createElement('img');
img.src = '/images/car-icon.png';
img.style.width = '32px';

// 加入標記
await this.manager!.addMarker({
  id: 'car_001',
  lon: 121.5654,
  lat: 25.0330,
  image: img,
  groupName: 'cars',
  groupLevel: 15,  // 縮放層級 >= 15 時顯示個別圖標
  groupColor: 'rgba(30,136,229,0.85)',  // 群聚泡泡顏色
  data: { carId: 'car_001', name: '車輛一' },
  onClick: (data) => {
    console.log('標記被點擊', data);
  },
});
```

#### 使用 Canvas 繪製動態圖示
```typescript
// 建立 SVG 車輛圖標（帶旋轉角度）
const createCarIcon = (angle: number, color: string) => {
  const canvas = document.createElement('canvas');
  canvas.width = 48;
  canvas.height = 48;
  const ctx = canvas.getContext('2d')!;

  ctx.save();
  ctx.translate(24, 24);
  ctx.rotate((angle * Math.PI) / 180);

  // 繪製車輛圖形
  ctx.fillStyle = color;
  ctx.beginPath();
  ctx.moveTo(-10, -15);
  ctx.lineTo(10, -15);
  ctx.lineTo(15, 0);
  ctx.lineTo(10, 15);
  ctx.lineTo(-10, 15);
  ctx.lineTo(-15, 0);
  ctx.closePath();
  ctx.fill();

  ctx.restore();
  return canvas;
};

// 使用
const icon = createCarIcon(90, '#ff0000');
await this.manager!.addMarker({
  id: 'car_002',
  lon: 121.5654,
  lat: 25.0330,
  image: icon,
  groupName: 'cars',
});
```

#### 差異更新群組標記
```typescript
// 更新整個群組的標記（適合定期刷新車輛位置）
const carMarkers: AddMarkerOptions[] = this.dataCars.map(car => ({
  id: car.Car_Id,
  lon: Number(car.Wgs_X),
  lat: Number(car.Wgs_Y),
  image: this.createCarIcon(car.course_arrow * 45, this.getCarColor(car.car_status_color)),
  groupName: 'cars',
  data: car,
  onClick: (data) => this.onCarClick(data),
}));

// 差異更新（只更新變更的標記）
this.manager!.syncGroup('cars', carMarkers);
```

### 2. 標記 Tooltip

```typescript
// 建立 Tooltip
const tooltip = this.manager!.setTooltip(
  '車輛資訊',  // 標題
  `<div>車號：${car.Car_Licence}</div>
   <div>速度：${car.SPEED} km/h</div>
   <div>狀態：${car.status_descr}</div>`,  // 內容
  {
    positioning: 'bottom-left',  // 定位方式
    offset: [10, -5],  // 偏移量
  }
);

// 顯示 Tooltip
tooltip.show(fromLonLat([car.Wgs_X, car.Wgs_Y]));

// 隱藏 Tooltip
tooltip.hide();

// 更新 Tooltip 內容（直接操作 DOM）
tooltip.element.querySelector('.marker-tooltip-body')!.innerHTML = '新內容';
```

### 3. 加入折線（Polyline）

```typescript
// 加入軌跡路線
this.manager!.addPolyline({
  id: 'route_001',
  points: [
    [121.5654, 25.0330],
    [121.5664, 25.0340],
    [121.5674, 25.0350],
  ],
  color: '#ff0000',
  width: 5,
  groupName: 'routes',
  groupLevel: 12,  // 縮放層級 >= 12 時顯示
  data: { routeName: '路線一' },
  onClick: (data) => console.log('折線被點擊', data),
});

// 取得折線控制代號
const polyline = this.manager!.getPolyline('route_001');
if (polyline) {
  // 開始動畫（圖標沿折線移動）
  const icon = document.createElement('img');
  icon.src = '/images/car-moving.png';
  polyline.startAnimation(icon, 5000);  // 5 秒完成一圈

  // 停止動畫
  polyline.stopAnimation();

  // 更新折線點位
  polyline.setPoints([[121.5654, 25.0330], [121.5664, 25.0340]]);

  // 手動觸發點擊事件
  polyline.dispatchEvent('click');
}
```

### 4. 加入多邊形（Polygon）

```typescript
// 加入圍籬區域
this.manager!.addPolygon({
  id: 'fence_001',
  rings: [
    [  // 外環
      [121.5654, 25.0330],
      [121.5664, 25.0330],
      [121.5664, 25.0340],
      [121.5654, 25.0340],
    ],
    // 可選：孔洞（內環）
    // [[121.5658, 25.0334], [121.5660, 25.0334], [121.5660, 25.0336], [121.5658, 25.0336]]
  ],
  fillColor: 'rgba(255, 0, 0, 0.3)',
  strokeColor: '#ff0000',
  strokeWidth: 2,
  groupName: 'fences',
  groupLevel: 14,  // 縮放層級 >= 14 時顯示多邊形，< 14 時顯示群聚泡泡
  groupColor: 'rgba(255, 140, 0, 0.85)',
  data: { fenceName: '圍籬一' },
  onClick: (data) => console.log('多邊形被點擊', data),
});

// 取得多邊形控制代號
const polygon = this.manager!.getPolygon('fence_001');
if (polygon) {
  // 開始動畫（圖標沿外環移動）
  const icon = document.createElement('img');
  icon.src = '/images/marker.png';
  polygon.startAnimation(icon, 5000);

  // 停止動畫
  polygon.stopAnimation();

  // 更新多邊形環狀點位
  polygon.setRings([[[121.5654, 25.0330], [121.5664, 25.0330], [121.5664, 25.0340]]]);

  // 手動觸發點擊事件
  polygon.dispatchEvent('click');
}
```

### 5. 標記管理

```typescript
// 取得標記控制代號
const marker = this.manager!.getMarker('car_001');
if (marker) {
  // 手動觸發點擊事件
  marker.dispatchEvent('click');
}

// 刪除單一標記
this.manager!.removeMarker('car_001');

// 刪除單一折線
this.manager!.removePolyline('route_001');

// 刪除單一多邊形
this.manager!.removePolygon('fence_001');

// 清除群組
this.manager!.clearGroup('cars');
this.manager!.clearPolylineGroup('routes');
this.manager!.clearPolygonGroup('fences');

// 清除所有標記
this.manager!.clearAll();
```

### 6. 滑鼠座標監聽

```typescript
// 監聽滑鼠移動取得經緯度
const unsubscribe = this.manager!.onMouseCoordinate((lon, lat) => {
  console.log(`經度: ${lon}, 緯度: ${lat}`);
});

// 取消監聽
unsubscribe();
```

## 地圖繪製功能（DrawManager）

### 1. 引入 DrawManager
```typescript
import { DrawManager } from '@/ol';
import type { DrawnFeatureData, DrawGeometryType } from '@/ol';

data() {
  return {
    drawManager: null as DrawManager | null,
    isDrawingMode: false,
    isModifyMode: false,
    isActivelyDrawing: false,
  };
}
```

### 2. 開始繪製
```typescript
startDrawing(type: DrawGeometryType) {
  // type: 'LineString' | 'Polygon'
  this.isDrawingMode = true;

  this.drawManager = new DrawManager(
    this.map!,
    type,
    (isDrawing) => {
      this.isActivelyDrawing = isDrawing;
    }
  );
}
```

### 3. 繪製控制
```typescript
// 完成當前繪製
completeDrawing() {
  this.drawManager?.completeDrawing();
}

// 回復上一點
undoDraw() {
  this.drawManager?.undoLastPoint();
}

// 切換修改模式
toggleModify() {
  if (this.isModifyMode) {
    this.drawManager?.disableModify();
    this.isModifyMode = false;
  } else {
    this.drawManager?.enableModify();
    this.isModifyMode = true;
  }
}

// 清除全部重新開始
clearAndRestartDraw() {
  this.drawManager?.clearAll();
}

// 停止繪製（不保留）
stopDrawing() {
  this.drawManager?.dispose();
  this.drawManager = null;
  this.isDrawingMode = false;
  this.isModifyMode = false;
  this.isActivelyDrawing = false;
}

// 確認送出
submitDraw() {
  const drawnData = this.drawManager?.getDrawnFeatures();
  if (drawnData && drawnData.length > 0) {
    console.log('繪製資料', drawnData);
    // 處理繪製的資料
    this.processDrawnData(drawnData);
  }
  this.stopDrawing();
}
```

## 完整 Vue 組件範例

```vue
<template>
  <Master01>
      <template #content>
        <!-- 查詢表單 -->
        <div class="control-panel">
          <Button label="查詢" icon="pi pi-search" @click="GetData" :loading="MapLoading" />
          <Button label="清除地圖" icon="pi pi-times" @click="clearMap" />
        </div>
      </template>
    </Card>

    <!-- 地圖容器 -->
    <iframe id="map" src="/map.html" style="width: 100%; height: 60vh;"></iframe>

    <!-- 資料列表 -->
    <Card style="height: calc(40vh - 130px); overflow-y:auto;">
      <template #content>
        <DataTable 
          :value="dataData"
          v-model:selection="dataSelected" 
          selectionMode="single"
          @row-select="onRowSelect">
          <Column field="Task_No" header="任務編號"></Column>
          <Column field="origin_addr" header="起點"></Column>
          <Column field="dest_addr" header="迄點"></Column>
        </DataTable>
      </template>
    </Card>
  </Master01>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import * as main from '@/main';
import Master01 from '@/master/master01.vue';

export default defineComponent({
  components: { Master01 },
  data() {
    return {
      oMpaRoot: null as any,
      MapLoading: true as boolean,
      dataData: [] as any[],
      dataSelected: null as any
    };
  },
  mounted() {
    this.MapInit();
  },
  watch: {
    // 監聽資料選擇變化
    dataSelected(newVal) {
      if (newVal) {
        let MapRoot = this.oMpaRoot;
        MapRoot.map.closeInfoWindow();
        this.openTaskInfoWindow(newVal);
      }
    }
  },
  methods: {
    /** 地圖初始化 */
    MapInit() {
      const mapElement = document.getElementById("map") as HTMLIFrameElement;

      if (mapElement) {
        mapElement.onload = () => {
          // 直接存取 iframe 內的 window 物件
          let m = mapElement.contentWindow as any;
          
          if (m && typeof m.map !== 'undefined') {
            this.oMpaRoot = m;
            
            // 設定地圖事件
            this.setupMapEvents();
            
            // 設定預設視圖
            m.map.ZoomXY(121, 23.5, 8);
            
            console.log("地圖已載入");
            this.MapLoading = false;
            
            // 載入初始資料
            this.loadInitialData();
          }
        };
      } else {
        console.error("找不到地圖元素");
      }
    },

    /** 設定地圖事件 */
    setupMapEvents() {
      let MapRoot = this.oMpaRoot;
      
      // 縮放限制
      MapRoot.map.attachEvent("zoomstart", () => {
        const zoom = MapRoot.map.getZoomLevel();
        if (zoom <= 8) MapRoot.map.ZoomXY(121, 23.5, 8);
        if (zoom >= 18) {
          setTimeout(() => MapRoot.map.setZoomLevel(18), 300);
        }
      });
    },

    /** 取得資料並繪製地圖 */
    GetData() {
      let MapRoot = this.oMpaRoot;
      let zoomX = 0;
      let zoomY = 0;
      const self = this;

      new main.clsAjaxAsync({
        loadingMsg: "Loading",
        url: main.clsConfig.ApiPath + "/Task/TaskMonitoring/GetData",
        body: { sdate: this.dataSearchForm.sdate },
        isPostModel: true,
        encrypt: true,
        async: true,
        success: (res: any) => {
          if (res.Result === true) {
            this.dataData = res.data;
            
            // 繪製任務標記
            this.dataData.forEach((item: any) => {
              let img = document.createElement('img');
              img.title = item.Task_No;
              
              // 根據載具類型選擇圖示
              if (item.CarType_id === "Drone") {
                img.src = "/images/drone_924717.png";
                img.style.width = "32px";
              } else {
                img.src = "/images/motorcycle_8343680.png";
                img.style.width = "48px";
              }

              zoomX += item.WGS_X;
              zoomY += item.WGS_Y;

              MapRoot.map.CreateMarker({
                id: "Task_" + item.Task_No,
                group: "Task",
                x: item.WGS_X,
                y: item.WGS_Y,
                marker: img,
                zindex: 10,
                data: item,
                click: function (e: any) {
                  self.openTaskInfoWindow(item);
                }
              });
            });

            // 縮放到所有標記的中心點
            if (this.dataData.length > 0) {
              MapRoot.map.ZoomXY(
                zoomX / this.dataData.length,
                zoomY / this.dataData.length,
                12
              );
            }
          }
        }
      }).ajax();
    },

    /** 開啟任務資訊視窗 */
    openTaskInfoWindow(item: any) {
      let MapRoot = this.oMpaRoot;
      MapRoot.map.closeInfoWindow();

      const htmlMsg = `
        <div style="min-width: 200px;">
          <div style="margin-bottom: 4px;">任務編號：${item.Task_No}</div>
          <div style="margin-bottom: 4px;">任務狀態：${item.TaskStatus_name}</div>
          <div style="margin-bottom: 4px;">配送日期：${new Date(item.Delivery_Date).format('yyyy-MM-dd')}</div>
          <div style="margin-bottom: 4px;">起點：${item.origin_addr}</div>
          <div style="margin-bottom: 4px;">迄點：${item.dest_addr}</div>
        </div>
      `;

      MapRoot.map.openInfoWindow(
        new MapRoot.dgXY(item.WGS_X, item.WGS_Y),
        htmlMsg
      );
      
      MapRoot.map.ZoomXY(item.WGS_X, item.WGS_Y, 15);
    },

    /** 清除地圖 */
    clearMap() {
      let MapRoot = this.oMpaRoot;
      MapRoot.map.RemoveAll();
    },

    /** 地圖重置（重新載入） */
    resetMap() {
      const timestamp = new Date().format("yyyyMMddhhmmss");
      document.getElementById("map")?.setAttribute("src", `/map.html?${timestamp}`);
    }
  }
});
</script>

<style scoped>
#map {
  width: 100%;
  height: 60vh;
  border: none;
}
</style>
```

## API 方法說明

### 地圖物件: `MapRoot.map`

| 方法 | 參數 | 說明 | 回傳 |
|------|------|------|------|
| **CreateMarker** | `{ id, x, y, marker, group?, zindex?, data?, click? }` | 建立標記（marker 為 HTMLElement 或 HTML 字串） | Promise |
| **CreateLine** | `{ id, points?, color?, width?, group?, zindex?, data? }` | 建立路線 | Promise |
| **CreatePolygon** | `{ id, points?, border_color?, background_color?, width?, group?, zindex?, data? }` | 建立多邊形 | Promise |
| **CreateCurve** | `{ id, center, ptR?, border_color?, background_color?, width?, ang1?, ang2?, clockw?, group?, zindex?, data? }` | 建立圓形/弧形 | Promise |
| **ZoomXY** | `(x, y, zoom)` | 縮放到指定座標 | void |
| **Zoom** | `(zoom)` | 設定縮放層級 | void |
| **getZoomLevel** | - | 取得當前縮放層級 | number |
| **setZoomLevel** | `(zoom)` | 設定縮放層級 | void |
| **openInfoWindow** | `(point, htmlString)` | 開啟資訊視窗 | void |
| **closeInfoWindow** | - | 關閉資訊視窗 | void |
| **attachEvent** | `(eventName, callback)` | 附加事件監聽器 | void |
| **RemoveObject** | `(id)` | 刪除指定物件 | void |
| **RemoveGroupObject** | `(group)` | 刪除群組所有物件 | void |
| **RemoveAll** | - | 刪除所有物件 | void |
| **Sort** | - | 依 zindex 重新排序圖層 | void |
| **GetPoints** | `(arrX, arrY)` | 產生點位陣列 | Array |
| **GetXY** | `(x, y)` | 產生單一點位 | dgXY |
| **GetObject** | `(id?)` | 取得物件 | Object |
| **GetGroupObject** | `(group)` | 取得群組物件 | Object |

### 地圖事件

| 事件名稱 | 觸發時機 |
|---------|---------|
| **zoomstart** | 縮放開始時 |
| **zoomend** | 縮放結束時 |
| **movestart** | 移動開始時 |
| **moveend** | 移動結束時 |

### 全域物件存取

```typescript
// 存取所有地圖物件
const allMapObjects = this.oMpaRoot.mObject;

// 存取所有群組
const allGroups = this.oMpaRoot.mGroup;

// 存取 dgXY 建構子
const point = new this.oMpaRoot.dgXY(121, 25);
```

## 實際應用範例

### 1. 任務監控系統
```typescript
methods: {
  /** 顯示任務與客戶位置 */
  async displayTasksAndCustomers() {
    let MapRoot = this.oMpaRoot;
    const self = this;
    let totalX = 0, totalY = 0;

    // 繪製客戶位置
    this.dataCustomer.forEach((customer: any) => {
      let img = document.createElement('img');
      img.src = "/images/customer_icon.png";
      img.title = customer.Customer_name;
      img.style.width = "48px";

      totalX += customer.WGS_X;
      totalY += customer.WGS_Y;

      MapRoot.map.CreateMarker({
        id: `Customer_${customer.Customer_no}`,
        group: "Customer",
        x: customer.WGS_X,
        y: customer.WGS_Y,
        marker: img,
        zindex: 5,
        data: customer,
        click: (e: any) => {
          self.openCustomerInfoWindow(customer);
        }
      });
    });

    // 繪製任務位置
    this.dataData.forEach((task: any) => {
      let img = document.createElement('img');
      img.title = task.Task_No;
      
      if (task.CarType_id === "Drone") {
        img.src = "/images/drone_924717.png";
        img.style.width = "32px";
      } else {
        img.src = "/images/motorcycle_8343680.png";
        img.style.width = "48px";
      }

      MapRoot.map.CreateMarker({
        id: `Task_${task.Task_No}`,
        group: "Task",
        x: task.WGS_X,
        y: task.WGS_Y,
        marker: img,
        zindex: 10,
        data: task,
        click: (e: any) => {
          self.openTaskInfoWindow(task);
        }
      });
    });

    // 縮放到所有標記的中心
    const count = this.dataCustomer.length;
    if (count > 0) {
      MapRoot.map.ZoomXY(totalX / count, totalY / count, 12);
    }
  }
}
```

### 2. 即時車輛追蹤
```typescript
methods: {
  /** 更新車輛位置 */
  updateVehiclePosition(vehicleId: string, x: number, y: number, speed: number, angle: number) {
    let MapRoot = this.oMpaRoot;
    
    let img = document.createElement('img');
    img.src = "/images/vehicle.svg";
    img.style.width = "40px";
    img.style.transform = `rotate(${angle}deg)`;
    img.title = `車輛 ${vehicleId} - 速度: ${speed} km/h`;

    MapRoot.map.CreateMarker({
      id: `vehicle_${vehicleId}`,
      group: "vehicles",
      x: x,
      y: y,
      marker: img,
      zindex: 15,
      data: { 
        vehicleId, 
        speed, 
        angle,
        lastUpdate: new Date() 
      }
    });
  },

  /** 開始即時追蹤 */
  startTracking() {
    this.trackingTimer = setInterval(() => {
      this.updateAllVehiclePositions();
    }, 5000);  // 每 5 秒更新一次
  },

  /** 停止即時追蹤 */
  stopTracking() {
    if (this.trackingTimer) {
      clearInterval(this.trackingTimer);
      this.trackingTimer = null;
    }
  }
}
```

### 3. 繪製配送路線
```typescript
methods: {
  /** 顯示任務路線 */
  displayTaskRoute(task: any) {
    let MapRoot = this.oMpaRoot;
    
    // 清除舊路線
    MapRoot.map.RemoveGroupObject('task_routes');
    
    // 建立路線點位
    if (task.routeCoordinates) {
      const arrX = task.routeCoordinates.map((c: any) => c.lng);
      const arrY = task.routeCoordinates.map((c: any) => c.lat);
      const points = MapRoot.map.GetPoints(arrX, arrY);
      
      // 繪製路線
      MapRoot.map.CreateLine({
        id: `route_${task.Task_No}`,
        group: 'task_routes',
        points: points,
        color: 'rgba(0, 122, 255, 1)',
        width: 5,
        zindex: 1,
        data: task
      });
    }
    
    // 新增起點標記
    let startImg = document.createElement('div');
    startImg.innerHTML = '??';
    startImg.style.fontSize = '30px';
    
    MapRoot.map.CreateMarker({
      id: `start_${task.Task_No}`,
      group: 'task_routes',
      x: task.origin_lng,
      y: task.origin_lat,
      marker: startImg,
      zindex: 2
    });
    
    // 新增終點標記
    let endImg = document.createElement('div');
    endImg.innerHTML = '??';
    endImg.style.fontSize = '30px';
    
    MapRoot.map.CreateMarker({
      id: `end_${task.Task_No}`,
      group: 'task_routes',
      x: task.dest_lng,
      y: task.dest_lat,
      marker: endImg,
      zindex: 2
    });
    
    // 重新排序圖層
    MapRoot.map.Sort();
  }
}
```

## 群組與圖層管理

### 群組結構建議
```typescript
const MAP_GROUPS = {
  AREAS: 'areas',           // zindex: 0 - 區域多邊形（最底層）
  ROUTES: 'routes',         // zindex: 1 - 路線
  CIRCLES: 'circles',       // zindex: 2 - 圓形範圍
  CUSTOMERS: 'Customer',    // zindex: 5 - 客戶位置
  TASKS: 'Task',            // zindex: 10 - 任務位置
  VEHICLES: 'vehicles'      // zindex: 15 - 車輛位置（最上層）
};
```

### 圖層管理最佳實踐
```typescript
// 1. 更新群組資料前先清除
MapRoot.map.RemoveGroupObject('Task');

// 2. 批次新增物件
tasks.forEach(task => {
  MapRoot.map.CreateMarker({ /* ... */ });
});

// 3. 最後統一排序
MapRoot.map.Sort();
```

## 進階技巧

### 1. 地圖重置
```typescript
// 完全重新載入地圖（清除所有狀態）
resetMap() {
  const timestamp = new Date().format("yyyyMMddhhmmss");
  document.getElementById("map")?.setAttribute("src", `/map.html?${timestamp}`);
}
```

### 2. 除錯與檢查
```typescript
// 檢查所有地圖物件
console.log('所有物件:', this.oMpaRoot.mObject);
console.log('所有群組:', this.oMpaRoot.mGroup);

// 檢查特定群組
console.log('任務群組:', this.oMpaRoot.mGroup.Task);

// 檢查地圖是否就緒
if (this.oMpaRoot && typeof this.oMpaRoot.map !== 'undefined') {
  console.log('地圖已就緒');
}
```

### 3. 動態標記樣式
```typescript
// 根據狀態改變標記圖示
const updateMarkerByStatus = (taskNo: string, status: string) => {
  let MapRoot = this.oMpaRoot;
  
  // 先取得原物件資料
  const oldObj = MapRoot.map.GetObject(`Task_${taskNo}`);
  const data = oldObj?.data;
  
  // 重新建立標記
  let img = document.createElement('img');
  
  switch(status) {
    case 'pending':
      img.src = "/images/marker_gray.png";
      break;
    case 'in_progress':
      img.src = "/images/marker_blue.png";
      break;
    case 'completed':
      img.src = "/images/marker_green.png";
      break;
    default:
      img.src = "/images/marker_default.png";
  }
  
  img.style.width = "48px";
  
  MapRoot.map.CreateMarker({
    id: `Task_${taskNo}`,
    group: "Task",
    x: data.WGS_X,
    y: data.WGS_Y,
    marker: img,
    zindex: 10,
    data: data
  });
};
```

## 注意事項

1. **地圖載入時機**: 
   - 必須等待 `iframe.onload` 事件完成後才能存取 `map` 物件
   - 建議使用 `MapLoading` 狀態管理載入狀態

2. **物件存取方式**:
   - ? 正確: `this.oMpaRoot.map.CreateMarker(...)`
   - ? 錯誤: 使用 `postMessage` 通訊

3. **Marker 參數格式**:
   - 優先使用 `document.createElement()` 建立 HTMLElement
   - 也支援 HTML 字串，會自動轉換為 HTMLElement

4. **點位格式**:
   - 使用 `new MapRoot.dgXY(x, y)` 建立點位物件
   - 或使用 `MapRoot.map.GetXY(x, y)` 輔助方法

5. **事件處理**:
   - Marker 的 `click` 事件需在建立時指定
   - 地圖事件使用 `attachEvent()` 監聽

6. **效能優化**:
   - 大量物件更新時，先移除舊物件再批次新增
   - 使用 `clearTimeout()` 避免重複繪製
   - 更新完成後呼叫 `Sort()` 統一排序

7. **座標系統**:
   - 使用 WGS84 座標系統（經度、緯度）
   - 經度 (x): 約 120-122（台灣）
   - 緯度 (y): 約 22-25（台灣）

8. **縮放層級限制**:
   - 建議最小層級: 8
   - 建議最大層級: 18
   - 可在 `zoomstart` 事件中限制範圍

## TypeScript 型別定義

```typescript
// 地圖根物件型別
interface MapRoot {
  map: EasyMapInstance;
  mObject: Record<string, any>;
  mGroup: Record<string, MapGroup>;
  dgXY: new (x: number, y: number) => any;
}

interface MapGroup {
  zindex: number;
  items: Record<string, any>;
}

interface EasyMapInstance {
  CreateMarker(params: CreateMarkerParams): Promise<void>;
  CreateLine(params: CreateLineParams): Promise<void>;
  CreatePolygon(params: CreatePolygonParams): Promise<void>;
  CreateCurve(params: CreateCurveParams): Promise<void>;
  ZoomXY(x: number, y: number, zoom: number): void;
  Zoom(zoom: number): void;
  getZoomLevel(): number;
  setZoomLevel(zoom: number): void;
  openInfoWindow(point: any, html: string): void;
  closeInfoWindow(): void;
  attachEvent(eventName: string, callback: Function): void;
  RemoveObject(id: string): void;
  RemoveGroupObject(group: string): void;
  RemoveAll(): void;
  Sort(): void;
  GetPoints(arrX: number[], arrY: number[]): any[];
  GetXY(x: number, y: number): any;
  GetObject(id?: string): any;
  GetGroupObject(group: string): any;
}

interface CreateMarkerParams {
  id: string;
  x: number;
  y: number;
  marker: HTMLElement | string;
  group?: string;
  zindex?: number;
  data?: any;
  click?: (e: any) => void;
}

interface CreateLineParams {
  id: string;
  points?: any[];
  color?: string;
  width?: number;
  group?: string;
  zindex?: number;
  data?: any;
}

interface CreatePolygonParams {
  id: string;
  points?: any[];
  border_color?: string;
  background_color?: string;
  width?: number;
  group?: string;
  zindex?: number;
  data?: any;
}

interface CreateCurveParams {
  id: string;
  center: any;
  border_color?: string;
  background_color?: string;
  ptR?: number;
  width?: number;
  ang1?: number;
  ang2?: number;
  clockw?: number;
  group?: string;
  zindex?: number;
  data?: any;
}
```

## 常見問題 FAQ

### Q1: 地圖載入失敗怎麼辦？
```typescript
MapInit() {
  const mapElement = document.getElementById("map") as HTMLIFrameElement;
  
  if (!mapElement) {
    console.error("找不到地圖元素");
    return;
  }

  mapElement.onload = () => {
    let m = mapElement.contentWindow as any;
    
    if (!m || typeof m.map === 'undefined') {
      console.error("地圖物件未載入");
      // 嘗試重新載入
      setTimeout(() => this.MapInit(), 1000);
      return;
    }
    
    this.oMpaRoot = m;
    this.MapLoading = false;
  };
  
  // 處理載入錯誤
  mapElement.onerror = () => {
    console.error("地圖載入錯誤");
    this.MapLoading = false;
  };
}
```

### Q2: 如何確保地圖方法呼叫順序？
```typescript
// 方法 1: 使用 async/await (CreateMarker 回傳 Promise)
async addMultipleMarkers() {
  let MapRoot = this.oMpaRoot;
  
  await MapRoot.map.CreateMarker({ id: 'marker1', /* ... */ });
  await MapRoot.map.CreateMarker({ id: 'marker2', /* ... */ });
  await MapRoot.map.CreateMarker({ id: 'marker3', /* ... */ });
  
  MapRoot.map.Sort();
}

// 方法 2: 批次新增後統一排序（推薦）
addMultipleMarkers() {
  let MapRoot = this.oMpaRoot;
  
  this.markers.forEach(marker => {
    MapRoot.map.CreateMarker({ /* ... */ });
  });
  
  // 等待所有標記建立完成後排序
  setTimeout(() => {
    MapRoot.map.Sort();
  }, 100);
}
```

### Q3: 標記點擊事件無效？
```typescript
// 確保使用 function 而非箭頭函數，並正確傳遞 this
const self = this;

MapRoot.map.CreateMarker({
  id: "marker_001",
  x: 121.5,
  y: 25.0,
  marker: img,
  click: function (e: any) {
    // 使用 self 存取 Vue 組件的 this
    self.handleMarkerClick(e);
  }
});
```

### Q4: 如何動態更新標記？
```typescript
// CreateMarker 會自動刪除同 ID 的舊標記再建立新的
updateMarker(id: string, newData: any) {
  let MapRoot = this.oMpaRoot;
  
  // 直接建立同 ID 的標記，舊的會被自動移除
  MapRoot.map.CreateMarker({
    id: id,
    x: newData.x,
    y: newData.y,
    marker: newData.markerElement,
    group: "dynamic_markers",
    zindex: 10,
    data: newData
  });
}
```

## 完整初始化流程

```typescript
export default defineComponent({
  data() {
    return {
      oMpaRoot: null as any,
      MapLoading: true as boolean
    };
  },
  mounted() {
    this.MapInit();
  },
  beforeUnmount() {
    // 清理計時器
    if (this.trackingTimer) {
      clearInterval(this.trackingTimer);
    }
  },
  methods: {
    MapInit() {
      const mapElement = document.getElementById("map") as HTMLIFrameElement;

      if (mapElement) {
        mapElement.onload = () => {
          let m = mapElement.contentWindow as any;
          
          if (m && typeof m.map !== 'undefined') {
            // 1. 儲存地圖根物件
            this.oMpaRoot = m;
            
            // 2. 設定地圖事件
            this.setupMapEvents();
            
            // 3. 設定預設視圖
            m.map.ZoomXY(121, 23.5, 8);
            
            // 4. 標記為已載入
            this.MapLoading = false;
            console.log("地圖已載入");
            
            // 5. 載入初始資料
            this.loadInitialData();
          }
        };
      }
    },

    setupMapEvents() {
      let MapRoot = this.oMpaRoot;
      
      // 縮放限制
      MapRoot.map.attachEvent("zoomstart", () => {
        const zoom = MapRoot.map.getZoomLevel();
        if (zoom <= 8) MapRoot.map.ZoomXY(121, 23.5, 8);
        if (zoom >= 18) {
          setTimeout(() => MapRoot.map.setZoomLevel(18), 300);
        }
      });
    },

    loadInitialData() {
      // 載入並顯示地圖資料
      this.GetData();
    }
  }
});
```

## 除錯技巧

```typescript
// 1. 檢查地圖是否載入
if (!this.oMpaRoot) {
  console.error('地圖未初始化');
  return;
}

// 2. 列出所有地圖物件
console.table(Object.keys(this.oMpaRoot.mObject));

// 3. 檢查特定物件
const obj = this.oMpaRoot.map.GetObject('marker_001');
console.log('物件資料:', obj?.data);

// 4. 檢查群組結構
console.log('群組結構:', this.oMpaRoot.mGroup);

// 5. 監控縮放層級
MapRoot.map.attachEvent("zoomend", () => {
  console.log('當前縮放層級:', MapRoot.map.getZoomLevel());
});
