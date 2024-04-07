●概要
Jsonc to ScriptableObject for UnityはjsonファイルからScriptableObjectを生成します。
UnityのJsonUtilityに小さな拡張を加えたものになります。

１．コメントのサポート
jsonc同様、jsonファイル内に"//"、"/*～*/"を利用してコメントを書く事ができます。

２．クラスの設定
json内に生成するScriptableObjectのクラスを記述する事ができます。これは以下の効果があります。
  ・jsonファイルをどのScriptableObjectクラスで生成するかをC#のコードを書く必要がなくなります。
  ・基底クラスの配列を持つScriptableObjectに対して、データに合わせて拡張クラスを生成できます。


●使い方
UnityのメニューからWindow > Json 2 ScriptableObject Editorを選択
ダイアログが表示されるので、出力したいjsoncファイルの入っているフォルダ名のボタンを選択して下さい。

●初期設定
Assets/Script/Json2Scriptable/Editor/J2SWindow.cs内の以下を設定して下さい。
　・DstDir:ScriptableObjectを出力するパス
　・SrcDir:元になるjsoncファイルを記載するパス

ScriptableObjectの生成は、上記フォルダの直下に作られたフォルダ単位で行われます。

 
 

  
