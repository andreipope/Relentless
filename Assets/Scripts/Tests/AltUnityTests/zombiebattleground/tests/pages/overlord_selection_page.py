from base import CZBTests
class Overlord_Selection_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.overlord_selection_page=self.get_overlord_selection_page()
        self.back_button=self.get_back_button()
        self.left_arrow_button=self.get_left_arrow_button()
        self.right_arrow_button=self.get_right_arrow_button()
        self.continue_button=self.get_continue_button()
        self.selected_overlord_text_object=self.get_selected_overlord_text_object()
    
    def get_overlord_selection_page(self):
        return self.altdriver.wait_for_element('Tab_SelectOverlord')
    def get_back_button(self):
        return self.altdriver.wait_for_element('Button_Back')
    def get_left_arrow_button(self):
        return self.altdriver.wait_for_element(self.overlord_selection_page.name+'/Panel_Content/Button_LeftArrow')
    def get_right_arrow_button(self):
        return self.altdriver.wait_for_element(self.overlord_selection_page.name+'/Panel_Content/Button_RightArrow')
    def get_continue_button(self):
        return self.altdriver.wait_for_element(self.overlord_selection_page.name+'/Panel_FrameComponents/Lower_Items/Button_Continue')
    def get_selected_overlord_text_object(self):
        return self.altdriver.wait_for_element(self.overlord_selection_page.name+'/Panel_Content/Text_SelectOverlord')

    def get_selected_overlord_name(self):
        return self.read_tmp_UGUI_text(selected_overlord_text_object)

    def press_left_arrow(self):
        self.button_pressed(self.left_arrow_button)
    def press_right_arrow(self):
        self.button_pressed(self.right_arrow_button)
    def press_continue(self):
        self.button_pressed(self.continue_button)
    def press_back(self):
        self.button_pressed(self.back_button)
    
    
    def select_overlord(self,overlord):
        started_overlord_name=self.get_selected_overlord_name()
        if started_overlord_name==overlord:
            return started_overlord_selected
        
        previous_overlord=started_overlord_name
        while True:
            self.press_right_arrow()
            started_overlord_name=self.get_selected_overlord_name()
            if started_overlord_name==overlord:
                return True
            if started_overlord_name==previous_overlord:
                return False
            previous_overlord=started_overlord_name


