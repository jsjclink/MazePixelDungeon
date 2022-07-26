# Maze Pixel Dungeon

게임 내에 보이지 않는 내용들에 대한 설명(던전 생성 방식, 맵 연결 구조 이미지 등)은
https://github.com/jsjclink/MazePixelDungeon/wiki 에 있습니다!

# 게임 소개
희대의 명작인 rouge의 게임을 따라 정통 roguelike 게임을 만들었습니다.

opensource 정통 roguelike 게임인 pixel dungeon을 모티브로 했고, 시간상 pixel dungeon의 asset을 가져와 썼습니다.

asset을 제외한 모든 게임 시스템은 독자적으로 만들었습니다.

# Maze pixel dungeon의 가장 큰 특징

1. pixel dungeon은 층마다 맵이 하나만 있습니다. (이 프로젝트 wiki 참고)
2. 하지만 maze pixel dungeon은 한 층에 여러 맵이 존재하고 이 맵들이 같은 층, 위 아래 층과 미로처럼 연결되어있습니다.

# 게임플레이

<p>
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/181493886-74afeb6a-cd9b-4077-90cd-00043581869b.jpg">
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/181493895-504404cd-4f59-411e-b0e7-9f99e396a09f.jpg">
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/181493929-aa8b7d0e-1059-4bdc-90eb-200e15b07cc6.jpg">
</p>

시작시 던전의 1-0층으로 내려가게 됩니다. 게임의 목표는 단순합니다. 던전의 적들을 처치하면서 아이템을 수집하고 끝까지 생존해 보스를 처치한 뒤 던전을 탈출하세요!

조작 방법은 간단합니다 : 터치

이 게임은 턴제 게임입니다. 플레이어가 움직이지 않으면 게임은 진행되지 않습니다.

턴마다 플레이어의 선택이 매우 중요해지는 상황이 있습니다. 위급한 상황이 왔을 때 신중히 생각한 후 행동하세요!

턴마다 게임이 세이브 됩니다. 신중하게 플레이하세요!

<p>
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/181494387-ec24c29c-1a54-4aea-b20e-3477bab7093a.jpg">
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/181494405-9b6d176a-f6dc-4f64-a976-37e13b87520d.jpg">
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/181494416-5020be19-4cd3-48dd-98cd-5312893be6e3.jpg">
</p>

<p>
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/181494445-319ec920-7e28-4d0d-a6c6-aaa17d78d532.jpg">
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/181494436-730fbf66-8f4e-4d4f-ab16-ec0a7abec80f.jpg">
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/181494484-10547076-875b-4f3b-a561-1a8e87874eed.jpg">
</p>

<p>
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/181494725-9851f60f-51c3-4ace-904e-6af7e9bdbd25.jpg">
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/181494494-30c448b8-e1e7-46f1-b6d3-b87051856642.png">
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/181494714-acf5849e-3071-4dab-beb6-fd3ca7173104.jpg">
</p>


# 기타

게임 시작 화면에 웅장한 bgm을 감상하세요

# wiki 일부 중요한 내용

모두 직접 짠 알고리즘/ 코드입니다!!!!!!!!!!!!!!!!

# 01 hierarachy와 layer, map이라는 개념
픽던 보면 1층-5층 6층-10층 과 같이 5층마다 맵 컨셉이 바뀌는데 이렇게 한 컨셉으로 묶인 층들을 hierarchy로 부르고
layer는 층
map은 실제 플레이 하는 맵을 의미함

# 01-1 layer

픽던은 한 층에서 내려가면 바로 다음층으로 내려가짐
이 게임은 한 층에 여러 맵이 존재하고 그 맵들이 다른 층, 같은 층과 연결돼있음

# 01-2 random layer generator
생성한 맵 구조가 다음과 같음

hierarchy1, 2가 연결된 모습

![layer](https://user-images.githubusercontent.com/60886172/180501936-011ecdd8-fd56-40a7-9681-ab8c89e0e5d1.JPG)

hierarchy1

![layer1](https://user-images.githubusercontent.com/60886172/180501968-9fa63e0d-f332-4021-a184-3745f83b57e0.JPG)

hierarchy2

![layer2](https://user-images.githubusercontent.com/60886172/180501977-51bea9ec-f106-4d64-badb-87085ea5becd.JPG)

# 01-2 map

random map generator를 만들어서 사각형 모양의 맵을 생성함

![map](https://user-images.githubusercontent.com/60886172/180502963-ee10604c-ece1-4a02-be8e-a8da71d8c9df.JPG)

<p>
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/180503015-4997f04d-89ec-4fcb-a3e0-dbddee4ea593.jpg">
  <img width="30%" height="30%" src="https://user-images.githubusercontent.com/60886172/180503023-01f8d141-8a39-4b57-aac1-3d6bbc2126fb.jpg">
</p>

