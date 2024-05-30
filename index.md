<html lang="ko">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>OverWind</title>
    <link href="https://fonts.googleapis.com/css2?family=Helvetica+Neue:wght@400;700&display=swap" rel="stylesheet">
    <style>
        body {
            font-family: 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
            color: #333;
        }

        header {
            background: linear-gradient(135deg, #667eea, #764ba2);
            color: #fff;
            padding: 2rem;
            text-align: center;
        }

        header h1 {
            margin: 0;
            font-size: 2.5rem;
        }

        .logo {
            max-width: 150px;
        }

        h2 {
            color: #764ba2;
            border-bottom: 2px solid #764ba2;
            padding-bottom: 0.5rem;
        }

        section {
            padding: 2rem;
            margin: 2rem auto;
            background: #fff;
            max-width: 900px;
            box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
            border-radius: 10px;
        }

        .images img {
            margin: 0.5rem;
            border-radius: 10px;
            transition: transform 0.3s;
            width: 100%;
            height: auto;
            max-width: 500px;
        }

        .images img:hover {
            transform: scale(1.05);
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
        }

        .badges img {
            margin: 0.5rem;
        }

        .center {
            text-align: center;
        }

        ul {
            list-style-type: none;
            padding: 0;
        }

        ul li {
            background: #f4f4f4;
            margin: 0.5rem 0;
            padding: 0.5rem;
            border-radius: 5px;
        }

        ul li a {
            color: #764ba2;
            text-decoration: none;
            font-weight: bold;
        }

        ul li a:hover {
            text-decoration: underline;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            margin: 2rem 0;
        }

        table, th, td {
            border: 1px solid #ddd;
        }

        th, td {
            padding: 1rem;
            text-align: center;
        }

        th {
            background-color: #764ba2;
            color: white;
        }

        footer {
            background: #333;
            color: #fff;
            text-align: center;
            padding: 1rem 0;
            position: fixed;
            width: 100%;
            bottom: 0;
        }

        .badge-img {
            width: 100px;
            height: 35px;
        }
    </style>
</head>
<body>
    <header>
        <h1>Over Wind</h1>
    </header>

    <section>
        <h2>프로젝트 소개</h2>
        <p>"바람은 계산하는것이 아니라 극복하는것이다."<br>영화 최종병기 활에서 나온 명언입니다. 플레이어들은 바람에 따라 변하는 궤도를 예측하며 도전 욕구를 끌어올릴 것입니다. 저희 "OverWind"는 예전에 큰 인기를 끌었던 포트리스 게임을 재현 및 재구성하여 포트리스류 게임의 부활을 목표로 합니다.</p>
        <div class="center images">
            <img alt="OverWind Login 화면" src="https://github.com/kookmin-sw/capstone-2024-33/assets/74590585/35fa36e4-7abb-48e7-b7b9-5736d1286eff">
            <img alt="OverWind 로비 화면" src="https://github.com/kookmin-sw/capstone-2024-33/assets/74590585/c4587748-60b5-4dc1-b450-9ae205d2d37c">
            <img alt="OverWind 팀 화면" src="https://github.com/kookmin-sw/capstone-2024-33/assets/74590585/89a1225b-e0d3-46ec-b1a0-5ba38be04ad2">
            <img alt="OverWind 인게임 화면" src="https://github.com/kookmin-sw/capstone-2024-33/assets/74590585/4d3f7f11-8ed1-491f-8368-6ee3af2f8e22">
        </div>

        <h2>발표 자료</h2>
        <ul>
            <li><a href="https://docs.google.com/presentation/d/1JO1jnwr7y5JQ6jxWW7t-PiPppP9xzBQy/edit?usp=drive_link&ouid=114998932213411349234&rtpof=true&sd=true" target="_blank" rel="noopener noreferrer">중간발표 pptx</a></li>
            <li><a href="https://docs.google.com/document/d/1UBRzcrMtn_R3fvz-Ow17mZ73ug6iCcqp/edit?usp=drive_link&ouid=114998932213411349234&rtpof=true&sd=true" target="_blank" rel="noopener noreferrer">중간 보고서 docs</a></li>
            <li><a href="https://docs.google.com/presentation/d/1W6f307y4nkLyZVSOoYnx0Ev64Ul9og_A/edit?usp=sharing&ouid=114998932213411349234&rtpof=true&sd=true" target="_blank" rel="noopener noreferrer">최종발표 pptx</a></li>
            <li><a href="https://drive.google.com/file/d/1b85_20fqHNlr-42ZiSO_1oisiCjMXWn_/view?usp=drive_link" target="_blank" rel="noopener noreferrer">포스터 ai 596x843</a></li>
            <li><a href="https://drive.google.com/file/d/1gJM2umaUDTVlD_pjLyQXy8TWORLPUBeS/view?usp=drive_link" target="_blank" rel="noopener noreferrer">포스터 png</a></li>
            <li><a href="https://docs.google.com/document/d/1GZ3a-qep1tCj5gTXBxU9SbI0QrJyfK3E/edit?usp=sharing&ouid=114998932213411349234&rtpof=true&sd=true" target="_blank" rel="noopener noreferrer">수행보고서_최종</a></li>
        </ul>

        <h2>팀 소개</h2>
        <table>
            <tr>
                <th scope="col">이름</th>
                <th scope="col">학번</th>
                <th scope="col">역할</th>
            </tr>
            <tr>
                <td>김민식</td>
                <td>XXXX1187</td>
                <td>팀전 구현 및 게임 모드 추가,<br>추가 수정을 통해 마지막 완성도를 높임</td>
            </tr>
            <tr>
                <td>박우진</td>
                <td>XXXX5162</td>
                <td>게임파트, UI디자인, 로비파트, 뒤끝서버</td>
            </tr>
            <tr>
                <td>정윤민</td>
                <td>XXXX3080</td>
                <td>게임파트, UI디자인, 로비파트, 뒤끝서버</td>
            </tr>
        </table>

        <h2>기술 스택</h2>
        <h3>Game</h3>
        <div class="badges">
            <img src="https://img.shields.io/badge/Unity-FFFFFF?style=for-the-badge&logo=Unity&logoColor=black">
            <img src="https://img.shields.io/badge/-C%23-000000?logo=Csharp&style=flat">
        </div>

        <h3>BackEnd</h3>
        <div class="badges">
            <img class="badge-img" src="https://github.com/kookmin-sw/capstone-2024-33/assets/74590585/67a49b5d-4956-49ab-b5df-3a990999f0fd">
            <img class="badge-img" src="https://github.com/kookmin-sw/capstone-2024-33/assets/74590585/a85e15e5-33ef-4256-bad3-dbd1e5b9aacf">
        </div>

        <h3>GIT Tools</h3>
        <div class="badges">
            <img src="https://img.shields.io/badge/github-181717?style=for-the-badge&logo=github&logoColor=white">
            <img src="https://img.shields.io/badge/git-F05032?style=for-the-badge&logo=git&logoColor=white">
        </div>

        <h2>게임 기술 소개</h2>
        <p>유니티, Photon, 뒤끝서버</p>

        <h2>팀 오버윈드 첫 구상 feat.Figma</h2>
        <div class="center">
            <img width="619" alt="팀 오버윈드 첫 구상" src="https://github.com/kookmin-sw/capstone-2024-33/assets/74590585/7cccd3f5-d478-4dbf-a0fc-8fb3ee0f8b6e">
            <br>
            <a href="https://www.figma.com/file/VrGnKXI2fd1GEfBjVr7mW1/Fortress-Project?type=design&mode=design&t=oQ4VLdoqE5T0LlQK-1" target="_blank" rel="noopener noreferrer">피그마 프로토타입 링크</a>
        </div>

        <h2>시연 영상</h2>
        <div class="center">
            <a href="https://youtu.be/UZhV_kJ3_T0" target="_blank" rel="noopener noreferrer">
                <img alt="OverWind 시연 영상 썸네일" src="https://github.com/kookmin-sw/capstone-2024-33/assets/74590585/4d3f7f11-8ed1-491f-8368-6ee3af2f8e22" width="500" height="300">
            </a>
        </div>

        <h2>설치 방법</h2>
        <p>맥용 윈도우용 실행파일 배포, 바로 실행 가능합니다.</p>
        <div>
            <a href="https://drive.google.com/file/d/1sdAqWcAkpWgphD7kpUfm35jnZfqsVGY0/view?usp=drive_link" target="_blank" rel="noopener noreferrer">For Mac (Intel + Apple Silicon)</a>
        </div>
        <div>
            <a href="https://drive.google.com/file/d/18kZK5VG15pp-iYEKncRIgZFmETpo6ahH/view?usp=drive_link" target="_blank" rel="noopener noreferrer">For Window 64</a>
        </div>
        <div>
            <a href="https://mega.nz/folder/081yTIha#-NftS7gSlOkb2sSfbqDb7Q" target="_blank" rel="noopener noreferrer">설치 불가시 대체 링크</a>
        </div>

        <h2>이후 추가할 내용</h2>
        <ol>
            <li>화살모드 추가</li>
        </ol>
    </section>

    <footer>
        <p>&copy; 2024 OverWind Team. All rights reserved.</p>
    </footer>
</body>
</html>
