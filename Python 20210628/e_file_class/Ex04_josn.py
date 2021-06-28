import json

f = open('./data/temp.json','r',encoding='utf-8')
json_data=f.read()
print(json_data)
print('---------------------------------')
data = json.loads(json_data)
print(data)

for item in data:
    print('>', item )
    # json은 딕셔너리 개념이라고 생각하고 다른 자료를 얻어오려면?
    # print(data[item])

    # for 문 이용하여 'No' 값과 'Job' 값도 출력한다면?
    # for val  in data[item]:
    #     print(data[item][val])
