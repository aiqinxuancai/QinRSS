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
1. 在宿主机新建数据目录（如 `./data`），将 `Config.yml` 放入其中。
2. 将 `webSocketLocation` 设置为 `ws://0.0.0.0:1868`，并确保容器端口映射到主机。
3. 运行容器，通过 `DATA_DIR` 环境变量指定数据目录：

```bash
mkdir -p ./data
cp QinRSS/Config.yml ./data/Config.yml
# 编辑 ./data/Config.yml ...

docker run -d --name qinrss \
  -p 1868:1868 \
  -v "$(pwd)/data:/data" \
  -e DATA_DIR=/data \
  -e TZ=Asia/Shanghai \
  ghcr.io/<owner>/<repo>:latest
```

> `DATA_DIR` 指定后，程序会将 `Config.yml`、`Subscription.json`、`Cache.json`、日志等所有数据文件统一读写到该目录，方便挂载 Volume 持久化。未设置时默认使用程序所在目录。

#### Docker Compose 示例
仓库根目录已提供 `docker-compose.yml` 示例，替换其中的镜像地址即可：

```bash
docker compose up -d
```

> 默认挂载 `./data` 到容器内 `/data` 并设置 `DATA_DIR=/data`，启动前请确保 `./data/Config.yml` 已准备好。

## 环境变量

| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| `DATA_DIR` | 数据目录，程序将在此目录读写 `Config.yml`、`Subscription.json`、`Cache.json` 及日志文件。Docker 部署时推荐挂载 Volume 并设置此变量。 | 程序所在目录 |
| `TZ` | 时区 | 系统时区 |

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
