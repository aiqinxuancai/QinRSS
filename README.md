# QinRSS
QinRSS是QQ机器人RSS订阅订阅插件，基于OneBot12协议，支持从RSSHub站点获取内容并发送到QQ群或QQ频道中。已在go-cqhttp的rc3中进行测试通过。
已支持使用ChatGPT翻译内容发送。

### 部署方法
下载对应系统的程序包，编辑Config.yml文件，

```yml
﻿#监听的ws地址
webSocketLocation: 'ws://127.0.0.1:1868'

#RSSHub站点地址，可自行寻找、搭建替换
rssHubUrl: 'https://rsshub.app'

#QQ群管理员ID
groupAdmins: [123456, 123456]

#QQ频道管理员ID
guildAdmins: ['123465', '123456']

# 离线超过1天后启动后首次不要发送订阅，避免消息轰炸 (废弃)
notSentAfterLongOffline: false

# 首次检测不发送，避免消息爆炸
firstCheckDontSend: true

# 检查订阅的时间间隔（秒），建议大于60秒，具体更新速度可能取决于RSSHub站点的设置
runInterval: 120

# 单次发送后等待的时间（秒），避免一些特殊的QQ客户端实现无法连续发送
sendInterval: 3

# 在插件中将图片下载后再进行发送，而非直接传递URL，避免部分情况go-cqhttp自身问题导致的图片无法正常发送
selfDownloadImage: false

# ImageProxy 图片代理，设置后使用代理下载图片发送，如 http://127.0.0.1:1080 ，仅在SelfDownloadImage设置为true时可用
imageProxy: ''

# OpenAI-Key，用于翻译内容时调用OpenAI
openAIKey: ''

# 用于无法连接OpenAI的情况
openAIProxy: ''

# 用于无法连接OpenAI的情况，架设的反代地址，有了反代就不建议设置openAIProxy了
openAIAPIBaseUri: ''
```

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

可在尾部附加参数为 --translateOnly仅发送翻译到中文后的内容，忽略原文

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
