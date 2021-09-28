# MOVEROAD_CHATTING_SERVER

@ 무브로드 ERP 프로그램의 채팅 서버 입니다 (https://github.com/sunggh/MoveRoad)
@ DEVELOPER. sunggh , seojeon9 , keemsangmin


@ Introduce 

  A. TCP 통신의 stream을 이용한 각 user 별 Room 개설, recieve, send 기능이 담긴 SERVER입니다.
  
  B. Message의 구조는 다음과 같습니다.
    a. opcode 
      1. join (user)
        ) + "|" +user_id(for db) 
      2. create room
        ) + "|" +user_id(me) + "|" + user_id(other person)
      3. Sending Message 
        ) + "|" + room_id + "|" + room.userid(target) + "|" + txt
    b. used
      . program.cs -> chatHandle
               
  C. Message가 보내지는 상황
    a. ERP프로그램에 유저가 접속할시
      1. join opcode를 자신을 제외한 접속중은 전체 stream에 전송 
    b. User가 상대방(target)을 지정하여 room을 만들시
      1. user와 target stream에 해당하는 room id를 전송
      2. erp 프로그램에서 자동 생성 완료 (ERP참고)  
    c. User가 txt를 send 할시
      
  D. Room이 만들어지는 메커니즘
    a. null 인 Room list를 생성
    b. ERP 프로그램에서 특정 패킷이 recv가 되어질시 Room 추가
     1. Room은 하나의 객체로 생성 되어있으며 index(room_id), userid, targetid로 구성되어있음
    c. 패킷을 send한 후에는 ERP프로그램 내부에서 FE으로 전송
    d. used 
      . Room.cs
      
