●概要  
Jsonc to ScriptableObject for UnityはjsonファイルからScriptableObjectを生成します。  
UnityのJsonUtilityに小さな拡張を加えたものになります。  

１．コメントのサポート  
jsonc同様、jsonファイル内に"//"、"/*～*/"を利用してコメントを書く事ができます。  
![comment_sample](https://github.com/seri-prg/Jsonc-to-ScriptableObject-for-Unity/assets/59523766/90efe503-8a20-4749-bcb6-875dfa061c22)

２．クラスの設定  
json内に生成するScriptableObjectのクラスを記述する事ができます。これは以下の効果があります。  
  ・jsonファイルをどのScriptableObjectクラスで生成するかをC#のコードを書く必要がなくなります。  
![create_object](https://github.com/seri-prg/Jsonc-to-ScriptableObject-for-Unity/assets/59523766/72824ad3-40a5-4f7d-8359-4aae3fd7cc4c)  
  ・基底クラスの配列を持つScriptableObjectに対して、データに合わせて拡張クラスを生成できます。  
![auto_class](https://github.com/seri-prg/Jsonc-to-ScriptableObject-for-Unity/assets/59523766/c6661dd0-fd71-43c4-820f-18a7c73ea1e9)
![class_auto_json](https://github.com/seri-prg/Jsonc-to-ScriptableObject-for-Unity/assets/59523766/57a558af-a48e-4eff-8c20-375eba9afa35)
![class_auto_obj](https://github.com/seri-prg/Jsonc-to-ScriptableObject-for-Unity/assets/59523766/3637fed7-d7b2-4b4a-b1d1-afa9eca86e79)


●使い方  
UnityのメニューからWindow > Json 2 ScriptableObject Editorを選択  
ダイアログが表示されるので、出力したいjsoncファイルの入っているフォルダ名のボタンを選択して下さい。  

●初期設定  
Assets/Script/Json2Scriptable/Editor/J2SWindow.cs内の以下を設定して下さい。  
　・DstDir:ScriptableObjectを出力するパス  
　・SrcDir:元になるjsoncファイルを記載するパス  

ScriptableObjectの生成は、上記フォルダの直下に作られたフォルダ単位で行われます。

 
 

  
