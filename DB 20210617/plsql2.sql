set serveroutput on
declare
   i number(4) := 1;
--if ���� then
--   ���
--[elsif ���� then
--   ���
--
--else ��� ]
--end if

-- break;    // ������ ����������

-- �ݺ��� : for ī��Ʈ���� in �ʱⰪ, �ִ밪 loop -- end loop;
-- �ݺ��� : while ���� loop ~~end loop;
-- �ݺ���: loop ~~end loop

-- while�� �ܼ��ݺ���(loop)���� �� 1~10 ���
begin
  	dbms_output.put_line('for�� �̿��� ó��');
   	for i in 1..10 loop
      		dbms_output.put_line('i�� �� -> ' || i);
   	end loop;

  	dbms_output.put_line('while�� �̿��� ó��');
   	while i<=10 loop
      		dbms_output.put_line('i�� �� -> '|| i);
		      i := i + 1;
   	end loop;
	i := 1; 
	dbms_output.put_line('loop�� �̿��� ó��');
   	loop
		dbms_output.put_line('i�� �� -> '|| i);
		i := i + 1;
      		--if i>10 then
		--    exit;
		--end if;     
		exit when i>10;
		
	  end loop;
end;
/


