# Unity Mobile RPG Prototype

2020~2021년에 제작한 Unity 3D RPG Prototype을 개선 및 기능 추가

시연 영상
- SinglePlay
https://youtu.be/xov9UZfR8Hk
- PvP
https://youtu.be/7lnXTwt9y-o

1. 개선점

1) LobbyManager 구조 개선
- Nickname, Room name 추가
- Ready 기능 추가

2) FSM 추가
- 기존의 enum 값에 따라 다르게 처리하는 방식에서 FSM을 도입
- Zombie의 AI, LobbyManager의 상태 처리 등에서 사용

3) GameMode 추가
- UE의 GameMode와 비슷하게 같은 씬이여도 GameMode에 따라 다른 게임을 진행할 수 있도록 구성
- SinglePlay, PvE, PvP 별로 구현

4) ObjectPool 개선
- 기존의 하나의 프리팹만 보관하던 방식에서 여러 프리팹을 보관하는 방식으로 변경

5) Rifle 연사 기능 추가
- 공격 버튼을 누를 때 연사가 가능하도록 변경

2. 스크린샷

![Screenshot_20220921-001838_RPG Prototype](https://user-images.githubusercontent.com/43817454/193445589-ed528d90-9975-4ab2-ad42-3c5fb9d8b6f0.jpg)
게임 시작 화면

![Screenshot_20220921-001846_RPG Prototype](https://user-images.githubusercontent.com/43817454/193445608-bc177623-cb35-469d-ac5b-bd4eb393e2d8.jpg)
닉네임 입력

![Screenshot_20220921-001858_RPG Prototype](https://user-images.githubusercontent.com/43817454/193445631-58c7ba26-4dde-4d8b-8800-8dbd9b25ee0b.jpg)
![Screenshot_20220929-163651_RPG Prototype](https://user-images.githubusercontent.com/43817454/193445639-e6065307-de79-4357-87bc-2d508858e555.jpg)
Room 리스트

![Screenshot_20220921-001904_RPG Prototype](https://user-images.githubusercontent.com/43817454/193445649-20545142-422d-4044-948d-86e97197a2d9.jpg)
Room 생성

![Screenshot_20220921-001913_RPG Prototype](https://user-images.githubusercontent.com/43817454/193445671-54dd8c5d-c39c-4ea6-81b3-5bd97ef98e48.jpg)
![Screenshot_20220929-163706_RPG Prototype](https://user-images.githubusercontent.com/43817454/193445693-bf173141-f608-4b4e-9376-f70e921df619.jpg)
Room

![Screenshot_20220921-001928_RPG Prototype](https://user-images.githubusercontent.com/43817454/193445733-9ae698f2-72c2-4414-806c-285c941d3f24.jpg)
![Screenshot_20220921-002053_RPG Prototype](https://user-images.githubusercontent.com/43817454/193445768-898fa464-51f7-453d-9128-f90c1ea6709e.jpg)
SinglePlay/PvE

![Screenshot_20220929-163718_RPG Prototype](https://user-images.githubusercontent.com/43817454/193445749-2d82d1da-bd95-49ba-aa60-9a90d217d3bf.jpg)
![Screenshot_20220929-163758_RPG Prototype](https://user-images.githubusercontent.com/43817454/193445756-6b49b260-9135-48d3-9fda-8b1da55ed96f.jpg)
![Screenshot_20220929-164037_RPG Prototype](https://user-images.githubusercontent.com/43817454/193445791-cf98e1bb-c6b6-4c21-9e58-8aee18d13d62.jpg)
PvP 
