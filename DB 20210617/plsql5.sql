set serveroutput on
--*���̺��� ���� ��������(������) : �÷����� ������ ����� �� ����
--select Į����,... inti ����,...from ���̺��,...;
accept num prompt '�Է� ->'
set verify off

declare
	name sawon.saname%type;
begin
	select saname into name from sawon where sabun =&num;
	dbms_output.put_line('�̸�:'|| name);
end;
/

