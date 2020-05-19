# Konane
Hawaiian Checker
Unity Project Version: 2019.3.3f1

## 程式介面
### Menu
<b>[New Game]</b> 開始新局。  
<b>[Resume]</b> 恢復先前的棋局。  
<b>[Exit]</b> 離開 App。  

### Game
依照標準 [夏威夷跳棋規則](https://brainking.com/cn/GameRules?tp=94) 遊玩。  
<b>[Return]</b> 可以放棄此回合、回到首頁選單。  
<b>[Hint(Eye)]</b> 可切換是否顯示所有目前可移動的棋子的最大移動步數。  

Hint:  
每回合皆會記錄當下棋局，若中途 [Return] 或 關掉 App，之後皆可透過 Menu [Resume] 恢復棋局  

## 打包
### Editor
<b>Tools/Build/android</b> - 輸出 Android APK  
<b>Tools/Build/standalone(windows)</b> - 輸出 Windows EXE  
<b>Tools/Build/all</b> - 同時輸出 Android APK & Windows EXE  

### CMD
#### Android APK
```
"C:\Program Files\Unity\Hub\Editor\2019.3.3f1\Editor\Unity.exe" -quit -batchmode -nographics -buildTarget Android -executeMethod GameBuilder.BuildViaCommandLine --buildingVersion "1.1.0" --buildingFolder ".\Output"
```

#### Windows EXE
```
"C:\Program Files\Unity\Hub\Editor\2019.3.3f1\Editor\Unity.exe" -quit -batchmode -nographics -buildTarget StandaloneWindows -executeMethod GameBuilder.BuildViaCommandLine --buildingVersion "1.1.0" --buildingFolder ".\Output"
```
