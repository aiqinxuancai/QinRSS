# QinRSS
QinRSS是基于OneBot12协议的订阅发送机器人，支持从RSSHub站点获取内容并发送到QQ群或QQ频道中。已在go-cqhttp的rc3中进行测试通过。

### 部署方法
下载对应系统的程序包，编辑Config.json文件，

```json
{
    "WebSocketLocation": "ws://127.0.0.1:1868",
    "RSSHubUrl": "https://rsshub.uneasy.win",
    "GroupAdmins": [123456, 123456],
    "GuildAdmins": ["123465", "123456"],
    "NotSentAfterLongOffline": false
}
```

* WebSocketLocation 监听的ws地址
* RSSHubUrl RSSHub站点地址，可自行寻找、搭建替换
* GroupAdmins QQ群管理员ID
* GuildAdmins QQ频道管理员ID
* NotSentAfterLongOffline 离线超过1天后启动后首次不要发送订阅，避免消息轰炸



在go-cqhttp中配置反向代理地址，然后运行QinRSS.exe

```yml
- ws-reverse:
    universal: ws://127.0.0.1:1868
```

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
