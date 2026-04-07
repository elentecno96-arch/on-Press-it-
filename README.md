# on-Press-it-
Team 9 Project 

프로젝트 이름 : Press It !

하이퍼 캐주얼 리듬 게임 (모바일)

- 개발 기간: 6개월
- 팀 구성: 5명
- 역할: 코어 시스템 설계 및 구현

## 목표
빠른 템포의 플레이와 직관적인 리듬 판정을 기반으로,
짧은 시간 안에 몰입할 수 있는 하이퍼 캐주얼 리듬 게임 제작

## 핵심 기능
- AudioSource 기반 타임라인 시스템 (CurrentTime)
- 판정 / 이벤트 / 스폰 시스템 분리 구조
- 연출과 로직 완전 분리 (연출 없이도 플레이 가능)
- UniTask 기반 비동기 초기화 및 씬 전환
- Firebase 기반 플레이어 데이터 저장/로드
- 스테이지 데이터 기반 구조 (확장 가능)
- 커스텀 에디터 툴을 통한 스테이지 시각화

## 시스템 구조
<img width="949" height="523" alt="Manager" src="https://github.com/user-attachments/assets/4156c481-df65-4929-b296-f51b0d651edd" />
<img width="872" height="708" alt="Core" src="https://github.com/user-attachments/assets/027e7f6a-a44b-41f3-9aea-eb32d8d38887" />

## 타임라인 기반 시스템
- AudioSource의 시간을 기준으로 CurrentTime 관리
- 모든 시스템이 동일한 시간 기준으로 동작

## 시스템 분리 구조
- JudgmentSystem → 판정 처리
- RhythmEventSystem → 이벤트 실행
- NoteSpawnSystem → 노트 생성

각 시스템이 독립적으로 동작하도록 설계

## 문제 해결 및 설계 개선 계획
### 1. CurrentTime 전역(static) 의존 문제
문제 : 모든 시스템이 static CurrentTime에 의존
해결 : ICurrentTime 인터페이스로 분리 예정
효과 : 테스트 가능, 시스템 독립성 확보

## 담당 역할
- 전체 코어 시스템 설계 및 구현
- 타임라인 기반 리듬 시스템 구현
- 판정 / 이벤트 / 스폰 시스템 설계
- 오디오 매니저 및 이벤트 연결 구조 개선
- Firebase 연동 (저장/로드)
- 에디터 툴 제작
- UI 및 시스템 통합
- QA 및 빌드 안정화

## 기술 스택
- Unity (C#)
- UniTask (비동기 처리)
- DoTween (연출)
- Firebase (데이터 저장)
- JSON (로컬 데이터 관리)

## 플레이 영상
링크 : 예정
