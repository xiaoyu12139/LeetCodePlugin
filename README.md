Visual studio 2026的leetcode插件
界面使用visual studio扩展编写，后端逻辑使用leetcode-cli这个项目（github上的开源项目）

修改菜单位置不生效：

```
1.vs中powershell中执行devenv /rootsuffix Exp /log，打开实验vs环境，删除之前的所有的插件
2.进入%APPDATA%\Microsoft\VisualStudio中找到对应的xx.xx_Exp\ComponentModelCache文件夹下的所有文件
3.然后以管理员打开vs，在vs中powershell中执行devenv /rootsuffix Exp /setup
```

修改vsct中的部分内容如下：

1.button标签部分

```
<Button guid="guidLeetCodePluginPackageCmdSet" id="ToolWindow1CommandId" priority="0x0100" type="Button">
		<Parent guid="guidLeetCodePluginPackageCmdSet1" id="MyMenuGroup" />
  <Icon guid="guidImages" id="bmpPic1" />
  <Strings>
    <ButtonText>LeetCodeExt</ButtonText>
  </Strings>
</Button>
```

2.Groups标签部分，在button同级

```
<Groups>
  <Group guid="guidLeetCodePluginPackageCmdSet1" id="MyMenuGroup" priority="0x0600">
    <Parent guid="guidSHLMainMenu" id="IDM_VS_MENU_TOOLS" />
  </Group>
</Groups>
```

3.GuidSymBol标签在Symbols标签下面

```
<GuidSymbol value="{469746fa-b449-4122-af42-28f79423a75f}" name="guidLeetCodePluginPackageCmdSet1">
  <IDSymbol value="4128" name="MyMenuGroup" />
</GuidSymbol>
```

4.扩展，管理，启用对应插件
