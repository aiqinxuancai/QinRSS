# QinRSS
QinRSS是QQ机器人RSS订阅订阅插件，基于OneBot12协议，支持从RSSHub站点获取内容并发送到QQ群或QQ频道中。已在go-cqhttp的rc3中进行测试通过。

### 部署方法
下载对应系统的程序包，编辑Config.json文件，

```json
{
  "WebSocketLocation": "ws://127.0.0.1:1868",
  "RSSHubUrl": "https://rsshub.app",
  "GroupAdmins": [ 123456, 123456 ],
  "GuildAdmins": [ "123465", "123456" ],
  "NotSentAfterLongOffline": false,
  "RunInterval": 120,
  "SelfDownloadImage": false,
  "ImageProxy": ""
}
```

* **WebSocketLocation** 监听的ws地址
* **RSSHubUrl** RSSHub站点地址，可自行寻找、搭建替换
* **GroupAdmins** QQ群管理员ID
* **GuildAdmins** QQ频道管理员ID
* **NotSentAfterLongOffline** 离线超过1天后启动后首次不要发送订阅，避免消息轰炸
* **RunInterval** 检查订阅的时间间隔（秒），建议大于60秒，具体更新速度可能取决于RSSHub站点的设置
* **SelfDownloadImage** 在插件中将图片下载后再进行发送，而非直接传递URL，避免部分情况go-cqhttp自身问题导致的图片无法正常发送
* **ImageProxy** 图片代理，设置后使用代理下载图片发送，如 http://127.0.0.1:1080，仅在SelfDownloadImage设置为true时可用


在go-cqhttp中配置反向代理地址，然后运行QinRSS.exe

```yml
- ws-reverse:
    universal: ws://127.0.0.1:1868
```
注意：频道主和QQ群主默认拥有操作订阅的权限

### 使用方法

#### 增加订阅
```
#add [自定义名称] [订阅地址如"twitter/user/TOUKEN_STAFF"]
```
可在尾部附加参数为 --translate 发送翻译到中文后的内容

#### 删除订阅
```
#remove [自定义名称]
```

#### 清空订阅
```
#clear
```
清除本频道、群中所有订阅

#### 查看订阅
```
#list
```
