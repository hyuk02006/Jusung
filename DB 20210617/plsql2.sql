set serveroutput on
declare
   i number(4) := 1;
--if 조건 then
--   명령
--[elsif 조건 then
--   명령
--
--else 명령 ]
--end if

-- break;    // 제어블록 빠져나가기

-- 반복문 : for 카운트변수 in 초기값, 최대값 loop -- end loop;
-- 반복문 : while 조건 loop ~~end loop;
-- 반복문: loop ~~end loop

-- while과 단순반복문(loop)으로 각 1~10 출력
begin
  	dbms_output.put_line('for를 이용한 처리');
   	for i in 1..10 loop
      		dbms_output.put_line('i의 값 -> ' || i);
   	end loop;

  	dbms_output.put_line('while를 이용한 처리');
   	while i<=10 loop
      		dbms_output.put_line('i의 값 -> '|| i);
		      i := i + 1;
   	end loop;
	i := 1; 
	dbms_output.put_line('loop를 이용한 처리');
   	loop
		dbms_output.put_line('i의 값 -> '|| i);
		i := i + 1;
      		--if i>10 then
		--    exit;
		--end if;     
		exit when i>10;
		
	  end loop;
end;
/


