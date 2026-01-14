# QinRSS
QinRSS 是 QQ 机器人 RSS 订阅插件，基于 OneBot 12 协议，支持从 RSSHub 获取内容并发送到 QQ 群或 QQ 频道。已在 go-cqhttp rc3 中测试通过，支持使用 ChatGPT 翻译内容后发送。

## 特性
- 基于 OneBot 12 协议
- 支持 RSSHub 订阅
- 支持 QQ 群与 QQ 频道
- 支持 ChatGPT 翻译推送

## 快速开始

### 方式一：直接运行
1. 下载对应系统的程序包（Release）。
2. 编辑 `Config.yml`。
3. 在 go-cqhttp 中配置反向代理地址，然后运行 `QinRSS.exe`。

```yml
- ws-reverse:
    universal: ws://127.0.0.1:1868
```

注意：频道主和QQ群主默认拥有操作订阅的权限。

### 方式二：Docker
1. 复制配置模板并修改：
   - `QinRSS/Config.yml` 是默认模板，可复制到仓库根目录后修改为 `Config.yml`。
2. 将 `webSocketLocation` 设置为 `ws://0.0.0.0:1868`，并确保容器端口映射到主机。
3. 运行容器：

```bash
docker run -d --name qinrss \
  -p 1868:1868 \
  -v "$(pwd)/Config.yml:/app/Config.yml:ro" \
  ghcr.io/<owner>/<repo>:latest
```

#### Docker Compose 示例
仓库根目录已提供 `docker-compose.yml` 示例，替换其中的镜像地址即可：

```bash
docker compose up -d
```

> 提示：Compose 已开启 `tty` 和 `stdin_open`，用于避免程序因标准输入关闭而退出。

## 配置说明（Config.yml）

```yml
# 监听的 ws 地址（Docker 中请使用 0.0.0.0）
webSocketLocation: 'ws://127.0.0.1:1868'

# RSSHub 站点地址
rssHubUrl: 'https://rsshub.app'

# QQ 群管理员 ID
groupAdmins: [123456, 123456]

# QQ 频道管理员 ID
guildAdmins: ['123465', '123456']

# 离线超过 1 天后启动首次不发送订阅 (废弃)
notSentAfterLongOffline: false

# 首次检测不发送，避免消息爆炸
firstCheckDontSend: true

# 检查订阅的时间间隔（秒）
runInterval: 120

# 单次发送后等待的时间（秒）
sendInterval: 3

# 在插件中将图片下载后再发送
selfDownloadImage: false

# 图片代理，仅在 selfDownloadImage 为 true 时生效
imageProxy: ''

# OpenAI Key，用于翻译内容
openAIKey: ''

# 无法直连 OpenAI 时可使用代理
openAIProxy: ''

# OpenAI API 反代地址（有反代时不建议再设置 openAIProxy）
openAIAPIBaseUri: ''

# OpenAI API 模型（可选）
openAIAPIModel: ''
```

## 使用方法

### 增加订阅
```
#add [自定义名称] [订阅地址如 "twitter/user/TOUKEN_STAFF"]
```
可在尾部附加参数 `--translate` 发送翻译到中文后的内容。

可在尾部附加参数 `--translateOnly` 仅发送翻译到中文后的内容，忽略原文。

### 删除订阅
```
#remove [自定义名称]
```

### 清空订阅
```
#clear
```
清除本频道/群中所有订阅。

### 查看订阅
```
#list
```

## 镜像发布（GitHub Actions）
推送 Git Tag 后，GitHub Actions 会自动构建并发布 Docker 镜像到 GHCR：

```
ghcr.io/<owner>/<repo>:<tag>
ghcr.io/<owner>/<repo>:latest
```
