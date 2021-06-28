import xml.etree.ElementTree as et

# temp.xml 먼저 확인
tree = et.ElementTree(file='./data/temp.xml')  # xml.etree.ElementTree.ElementTree 인 것이다
root = tree.getroot()
print('root:', root)
print('root.tag:', root.tag)

for child in root:
    # print(child.tag)    # employee 출력
    for grandchild in child:
        print(grandchild.tag, '/',  grandchild.text)

"""
temp2.xml 확인하면 속성값이 있다
print(grandchild.tag, '/', grandchild.attrib,'/', grandchild.text)
"""

# [참고] http://egloos.zum.com/sweeper/v/3045370

# [연습] sample.xml 파일에서 각 항목별 총합과 전체 항목의 총합 구하기